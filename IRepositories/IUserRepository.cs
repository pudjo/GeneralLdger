using Accounting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    public interface IUserRepository:IRepository<User>
    {
        Task<List<User>> GetAllAsync();
        
    }
}
