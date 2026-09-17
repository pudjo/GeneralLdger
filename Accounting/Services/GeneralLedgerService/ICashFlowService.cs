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
    internal interface ICashFlowService
    {

        event EventHandler? CashFlowChanged;
        
        Task<int> CreateCustomerAsync(ContactDTO dto, string createdBy);
        Task<List<CashFlowItemDTO>> GetTreeCashFlowItems();
        Task<List<CashFlowItemDTO>> GetCashFlowItemsDTOAsync();
        Task<bool> SaveCashFlowItemAsync(CashFlowItemDTO CashFlowItemDto);
                    }
    }





