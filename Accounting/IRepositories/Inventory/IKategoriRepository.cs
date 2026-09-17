using Accounting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.Inventory
{
    internal interface IKategoriRepository : IRepository<Kategori>
    {
        Task<int> ImportBunch(List<Kategori> list);
    }

}
