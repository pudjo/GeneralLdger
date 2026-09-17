
using Accounting.Domain.Entities;

namespace Accounting.IRepositories
{
    public interface IAccountQueryRepository
    {
        Task<List<Account>> GetAllAsync();
        Task<Account> GetByIdAsync(string Id);

    }
}
