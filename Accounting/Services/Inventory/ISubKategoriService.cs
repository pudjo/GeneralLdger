using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal interface ISubKategoriService
    {
        
        Task<List<SubKategoriDTO>> GetAllAsync();
        Task<SubKategoriDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(SubKategoriDTO dto);
        Task<bool> UpdateAsync(SubKategoriDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}

