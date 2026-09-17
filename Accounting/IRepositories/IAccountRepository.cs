using Accounting.Domain.Entities;
using Accounting.DTO;

namespace Accounting.IRepositories
{
    public  interface IAccountRepository : IRepository<Account>
    {
        void PerbaikiLevel(int level);
        Task<int> ImportBunch(List<Account> lstAccount);
       Task<bool>DeleteAsync(string code);



    }
}
