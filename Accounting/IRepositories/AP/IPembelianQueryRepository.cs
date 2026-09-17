using Accounting.Domain.Entities.AP;
using Accounting.DTO.AP;
using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.AP
{
    internal interface IPembelianQueryRepository
    {
        Task<List<PembelianDTO>> GetAllAsync();
        Task<PembelianDTO?> GetByIdAsync(int id);
        Task<List<PembelianDTO>> GetByVendorIdAsync(int VendorId);
        Task<List<PembelianDTO>> SearchAsync(string keyword);

    }
}
