using Accounting.Domain.Entities;
using Accounting.IRepositories;
using System.Data;

namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class GeneralLedgerRepository : BaseRepository, IGeneralLedgerRepository
    {
        Func<IDbConnection> connectionFactory;

        public GeneralLedgerRepository(Func<IDbConnection> _connectionFactory, Func<IDbTransaction> transactionFactory)
            : base(_connectionFactory, transactionFactory)
        {
            connectionFactory = _connectionFactory;
            // Konstruktor ini boleh kosong karena datanya langsung dioper ke : base()
        }
        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<int> IRepository<GeneralLedger>.CreateAsync(GeneralLedger entity)
        {
            throw new NotImplementedException();
        }

        Task<int> IRepository<GeneralLedger>.UpdateAsync(GeneralLedger entity)
        {
            throw new NotImplementedException();
        }
    }
}
