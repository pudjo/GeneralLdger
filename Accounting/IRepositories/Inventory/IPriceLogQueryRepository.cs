using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.Inventory
{
    internal interface IPriceLogQueryRepository
    {
        Task<List<PriceLogDTO>> GetAllAsync();
        Task<List<PriceLogDTO>> GetByProductIdAsync(int productId);
        Task<PriceLogDTO?> GetLatestByProductIdAsync(int productId);

    }
}
