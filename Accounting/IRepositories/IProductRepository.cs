using Accounting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    internal interface IProductRepository : IRepository<Product>
    {
        Task<int> ImportBunch(List<Product> products);

    }
}
