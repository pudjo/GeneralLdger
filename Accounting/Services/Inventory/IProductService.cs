using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    public  interface IProductService
    {
        Task<List<ProductDTO>> GetAllAsync();
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<ProductDTO?> GetByCodeAsync(string code);
        Task<List<ProductDTO>> SearchAsync(string keyword);
        Task<List<ProductDTO>> GetByJenisAsync(int jenisId);
        Task<int> CreateAsync(ProductDTO dto, string createdBy);
        Task<bool> UpdateAsync(ProductDTO dto, string updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<int> ImportBunchAsync(List<ProductDTO> dtos);

        event EventHandler? ProductsChanged;

    }
}
