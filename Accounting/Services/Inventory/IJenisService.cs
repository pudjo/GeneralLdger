using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal interface IJenisService
    {
        Task<List<JenisDTO>> GetAllAsync();
        Task<JenisDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(JenisDTO dto);
        Task<bool> UpdateAsync(JenisDTO dto);
        Task<bool> DeleteAsync(int id);

    }
}
