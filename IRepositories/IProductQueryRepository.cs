using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    internal interface IProductQueryRepository
    {
        Task<List<ProductDTO>> GetAllAsync();
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<ProductDTO?> GetByCodeAsync(string code);
        Task<List<ProductDTO>> SearchAsync(string keyword);
        Task<List<ProductDTO>> GetByJenisAsync(int jenisId);
    }
}

