using Accounting.Domain;
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite;
using ControlzEx.Standard;

namespace Accounting.Services.GeneralLedgerService
{
    internal class SaldoAwalService
    {
        private IWriteSaldoAwalRepository writeSaldoAwalRepository;
        private ISaldoAwalReadRepository saldoAwalReadRepository;
        private IAccountQueryRepository accountQueryRepository;
        private List<GeneralLedgerDTO> generalLedgerDTOs = new List<GeneralLedgerDTO>();

        IUnitOfWork unitOfWork;
        public SaldoAwalService(IUnitOfWork _unitOfWork, ISaldoAwalReadRepository saldoAwalReadRepository)
        {
            unitOfWork = _unitOfWork;
            this.saldoAwalReadRepository = saldoAwalReadRepository;
        }

        public async Task<int> Save(List<SaldoAwalDTO> listsaldoawalDTO)
        {

            int totalRowAffected = 0;
            try
            {
                List<SaldoAwalDTO> lstSaldoAwal = await saldoAwalReadRepository.GetOnYear(AppSession.SelectedYear); 

                await unitOfWork.BeginTransactionAsync();
                // Mapping dari DTO ke Entity SaldoAwal
                foreach (SaldoAwalDTO saldoawalDTO in listsaldoawalDTO)
                {
                    SaldoAwal saldoawal = new()
                    {
                        Id = saldoawalDTO.Id,
                        Year = AppSession.SelectedYear,
                        AccountCode = saldoawalDTO.AccountCode,
                        Debet = saldoawalDTO.Debet,
                        Credit = saldoawalDTO.Credit,

                    };
                    SaldoAwalDTO saldoAwalDTO = new SaldoAwalDTO();
                    if (lstSaldoAwal != null)
                    {
                        saldoAwalDTO= lstSaldoAwal.FirstOrDefault(x => x.AccountCode.Trim() == saldoawalDTO.AccountCode.Trim());  
                    }
                    int  result = 0;  
                    // Cek apakah sudah ada di database 
                    // berdasar tahun dan kode 
                    //

                    if (saldoAwalDTO == null)
                    {
                        result = await unitOfWork.WriteSaldoAwalRepository.CreateAsync(saldoawal);
                    }
                    else
                    {
                        result = await unitOfWork.WriteSaldoAwalRepository.UpdateAsync(saldoawal);
                    }

                    totalRowAffected += result;

                }
                await unitOfWork.CommitAsync();
                
                return totalRowAffected;
            }
            catch (Exception ex)
            {
                // Jika ada error di tengah jalan, kembalikan database ke kondisi semula
                await unitOfWork.RollbackAsync();

                // Catat log error ex disini jika ada logger
                return 0;
            }
        }
        public async Task<bool> Delete(int id)
        {

            try
            {
                bool result = false;
                result = await writeSaldoAwalRepository.DeleteAsync(id);
                
                return result;
            }
            catch (Exception ex)
            {
                // Jika ada error di tengah jalan, kembalikan database ke kondisi semula
                await unitOfWork.RollbackAsync();

                // Catat log error ex disini jika ada logger
                return false;
            }
        }
    }
}
