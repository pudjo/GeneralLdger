using Accounting.Domain.Entities;
using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    public interface IRepository<T> where T : class
    {
            Task<int> CreateAsync(T entity);
            Task<int> UpdateAsync(T entity);
            Task<bool> DeleteAsync(int id);
    }       
}
