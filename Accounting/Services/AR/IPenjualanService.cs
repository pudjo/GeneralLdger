using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.AR
{
    public  interface IPenjualanService
    {
        Task<int> CreateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details);
        Task<bool> UpdateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details);
        Task<bool> DeleteAsync(int id);

        // Read/query methods
        Task<List<PenjualanDTO>> GetAllAsync();
        Task<PenjualanDTO?> GetByIdAsync(int id);
        Task<List<PenjualanDTO>> GetByCustomerIdAsync(int customerId);
        Task<List<PenjualanDTO>> SearchAsync(string keyword);
    }
}
