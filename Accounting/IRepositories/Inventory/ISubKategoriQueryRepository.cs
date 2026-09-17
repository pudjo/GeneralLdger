using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.Inventory
{
    internal interface ISubKategoriQueryRepository
    {
        Task<List<SubKategoriDTO>> GetAllAsync();
        Task<SubKategoriDTO?> GetByIdAsync(int id);
        Task<List<SubKategoriDTO>> GetByKategoriIdAsync(int kategoriId);
        Task<List<SubKategoriDTO>> SearchAsync(string keyword);

    }
}
