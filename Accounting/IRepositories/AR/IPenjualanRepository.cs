using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.AR
{
    interface IPenjualanRepository
    {
        Task<int> CreateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details);
        Task<int> UpdateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details);
        Task<bool> DeleteAsync(int id);

    }
}
