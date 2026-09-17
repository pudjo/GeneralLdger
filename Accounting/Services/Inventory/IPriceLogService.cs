using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal interface IPriceLogService
    {
        Task<bool> SavePriceAsync(int productId, decimal newHargaJual, int createdBy);

    }

}
