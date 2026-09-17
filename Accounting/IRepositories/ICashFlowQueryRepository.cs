using Accounting.Domain.Entities;
using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    public  interface ICashFlowQueryRepository
    {
        Task<List<CashFlowItemDTO>> GetAllAsync();
        Task<CashFlowItemDTO> GetByIdAsync(string Code);
        
    }
}
