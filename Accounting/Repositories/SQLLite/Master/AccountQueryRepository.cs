using Accounting.Domain.Entities;
using Accounting.IRepositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Master
{
    internal class AccountQueryRepository:  IAccountQueryRepository
    {
        Func<IDbConnection> connectionFactory;
        public AccountQueryRepository(Func<IDbConnection> _connectionFactory)
        {
            connectionFactory = _connectionFactory;
        }
        public async Task<List<Account>> GetAllAsync()
        {
            try
            {
                var query = "SELECT * from Account order by Account.ID";

                using (IDbConnection connection = connectionFactory())
                {
                    var lstAccount = await connection.QueryAsync<Account>(query).ConfigureAwait(false);
                    return lstAccount.ToList();

                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }
        public async Task<Account> GetByIdAsync(string Id)
        {
            try
            {
                var query = "SELECT * FROM  Account WHERE Id = @Id";
                var p = new DynamicParameters();
                using (IDbConnection connection = connectionFactory())
                {
                    var account = connection.QuerySingle<Account>(query, new { Id = Id });
                    return account;
                }
            }
            catch (Exception exp)
            {
                
                return null;
            }
        }
    }
}
