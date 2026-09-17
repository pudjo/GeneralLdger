using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.Inventory
{
    internal interface IJenisQueryRepository
    {
        Task<List<JenisDTO>> GetAllAsync();
        Task<JenisDTO?> GetByIdAsync(int id);
        Task<List<JenisDTO>> SearchAsync(string keyword);

    }
}
