using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.GeneralLedgerService
{

     public class CashFlowService:ICashFlowService
    {
        private ICashFlowItemRepository cashFlowItemRepository;
        private ICashFlowQueryRepository cashFlowQueryRepository;
        private readonly ILogger<CashFlowService> _logger;

        public event EventHandler? CashFlowChanged;
        
        public CashFlowService(ICashFlowItemRepository _cashFlowItemRepository, ICashFlowQueryRepository _cashFlowQueryRepository, ILogger<CashFlowService> logger)
        {

            cashFlowItemRepository = _cashFlowItemRepository;
            cashFlowQueryRepository = _cashFlowQueryRepository;
            _logger = logger;
        }

        private void OnCashFlowChanged() => CashFlowChanged?.Invoke(this, EventArgs.Empty);

        public async Task<List<CashFlowItemDTO>> GetTreeCashFlowItems()
        {
            try
            {

                var lstCashFlowItem = await GetCashFlowItemsDTOAsync();
                List<CashFlowItemDTO> lst = lstCashFlowItem.ToList();
                List<CashFlowItemDTO> lstReturned = new List<CashFlowItemDTO>();
                foreach (CashFlowItemDTO CashFlowItem in lst)
                {
                    if (string.IsNullOrEmpty(CashFlowItem.ParentCode) || CashFlowItem.ParentCode.Trim() == "0")
                    {
                        // Recusively call it's children
                        CashFlowItem.Children = GetChildren(lst, CashFlowItem.Code);
                        lstReturned.Add(CashFlowItem);
                    }

                }
                _logger.LogInformation("Get data from database..");

                return lstReturned;


            }
            catch (Exception exp)
            {
                // Mencatat error secara detail beserta Stack Trace-nya
                _logger.LogError(exp, "Error in GetTreeCashFlowItems  IdParent: {Id}", "Root");
                throw;
            }
        }
        private ObservableCollection<CashFlowItemDTO> GetChildren(List<CashFlowItemDTO> lst, string idParent)
        {
            var lstChildren = new ObservableCollection<CashFlowItemDTO>();
            try
            {
                if (string.IsNullOrEmpty(idParent)) return lstChildren;

                var matches = lst.Where(a => a.ParentCode == idParent).ToList();

                foreach (CashFlowItemDTO cashFlowItem in matches)
                {
                    cashFlowItem.Children = GetChildren(lst, cashFlowItem.Code);
                    lstChildren.Add(cashFlowItem);
                }
                return lstChildren;
            }
            catch (Exception exp)
            {
                _logger?.LogError(exp, "Error building children for ParentCode: {Id}", idParent);
                return lstChildren;
            }
        }

        public async Task<List<CashFlowItemDTO>> GetCashFlowItemsDTOAsync()
        {

            IEnumerable<CashFlowItemDTO> rawCashFlowItems = await cashFlowQueryRepository.GetAllAsync();
            List<CashFlowItemDTO> CashFlowItemList = rawCashFlowItems.ToList();

            List<CashFlowItemDTO> dtoList = CashFlowItemList.Select(current => new CashFlowItemDTO
            {
                Code = current.Code,
                Name = current.Name,
                ParentCode = current.ParentCode,
                ParentName = current.ParentName,
                GroupType = current.GroupType,
            }).ToList();

            return dtoList;
        }

        public async Task<bool> SaveCashFlowItemAsync(CashFlowItemDTO CashFlowItemDto)
        {
            // 1. TUGAS SERVICE: Validasi Aturan Bisnis (Akuntansi)


            if (string.IsNullOrEmpty(CashFlowItemDto.Name))
            {
                throw new Exception("Nama arus kas wajib diisi demi menjaga opini WTP!");
            }

            CashFlowItem cashFlowItem = new CashFlowItem
            {
                Code = CashFlowItemDto.Code,
                Name = CashFlowItemDto.Name,
                ParentCode = CashFlowItemDto.ParentCode,
                GroupType = CashFlowItemDto.GroupType,

            };
            int rowsAffected = 0;

            try
            {
                // PERBAIKAN: pastikan menunggu GetByIdAsync dan gunakan hasilnya untuk menentukan Create/Update
                var existing = await cashFlowQueryRepository.GetByIdAsync(cashFlowItem.Code);

                if (existing == null)
                {
                    rowsAffected = await cashFlowItemRepository.CreateAsync(cashFlowItem);
                }
                else
                {
                    rowsAffected = await cashFlowItemRepository.UpdateAsync(cashFlowItem);
                }
                if (rowsAffected > 0)
                {
                    OnCashFlowChanged();
                }
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Error saving cashflow item {Code}", cashFlowItem.Code);
                throw;
            }

            return rowsAffected > 0;
        }

        public Task<int> CreateCustomerAsync(ContactDTO dto, string createdBy)
        {
            throw new NotImplementedException();
        }
    }
}


