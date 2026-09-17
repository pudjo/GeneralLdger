using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.Inventory
{
    internal interface IKategoriQueryRepository
    {
        Task<List<KategoriDTO>> GetAllAsync();
        Task<KategoriDTO?> GetByIdAsync(int id);
        Task<KategoriDTO?> GetByNameAsync(string name);
        Task<List<KategoriDTO>> SearchAsync(string keyword);

    }
}
