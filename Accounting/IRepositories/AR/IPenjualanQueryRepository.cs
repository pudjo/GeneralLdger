using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.AR
{
    internal interface IPenjualanQueryRepository
    {
        Task<List<PenjualanDTO>> GetAllAsync();
        Task<PenjualanDTO?> GetByIdAsync(int id);
        Task<List<PenjualanDTO>> GetByCustomerIdAsync(int customerId);
        Task<List<PenjualanDTO>> SearchAsync(string keyword);

    }
}
