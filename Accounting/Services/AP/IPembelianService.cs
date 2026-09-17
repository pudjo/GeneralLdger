using Accounting.DTO.AP;
using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.AP
{
    internal interface IPembelianService
    {
        Task<int> CreateAsync(PembelianDTO Pembelian, IEnumerable<PembelianDetailDTO> details);
        Task<bool> UpdateAsync(PembelianDTO Pembelian, IEnumerable<PembelianDetailDTO> details);
        Task<bool> DeleteAsync(int id);

        // Read/query methods
        Task<List<PembelianDTO>> GetAllAsync();
        Task<PembelianDTO?> GetByIdAsync(int id);
        Task<List<PembelianDTO>> GetByVendorIdAsync(int VendorId);
        Task<List<PembelianDTO>> SearchAsync(string keyword);
    }
}
