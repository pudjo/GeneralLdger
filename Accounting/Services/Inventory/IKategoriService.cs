using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal interface  IKategoriService
    {
        Task<List<KategoriDTO>> GetAllAsync();
        Task<KategoriDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(KategoriDTO dto);
        Task<bool> UpdateAsync(KategoriDTO dto);
        Task<bool> DeleteAsync(int id);

    }
}
