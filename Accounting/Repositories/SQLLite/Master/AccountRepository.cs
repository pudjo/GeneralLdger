using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite.ERP;
using Dapper;

using Microsoft.Extensions.Logging;
using System.Data;

namespace Accounting.Repositories.SQLLite.Master
{
    internal class AccountRepository : IAccountRepository
    {
        private readonly ILogger<AccountRepository> _logger;
        private Func<IDbConnection> connectionFactory;
        public AccountRepository(Func<IDbConnection> _connectionFactory ,ILogger<AccountRepository> logger)
        {
            connectionFactory = _connectionFactory;
            _logger = logger;
        }
        public void PerbaikiLevel(int level)
        {

            //level adalah level yang di tuju
            // sehingga mencarinya 
            List<Account> lstOnLevelMinus1 = new List<Account>();
            try
            {
                var p = new DynamicParameters();
                p.Add("@level", level);

                // dapatkam accounyt pada level ini.
                var query = "SELECT Account.ID,Account.Root,Account.Leaf, Account.Name,Account.Debet ,Account.IdParent  FROM Account  where  Root=@level -1 order by Account.ID";
                using (IDbConnection connection = connectionFactory())
                {
                    var lstAccount = connection.Query<Account>(query, p);
                    // var people = cnn.Query<PersonModel>(sql, p);
                    foreach (Account acc in lstAccount)
                    {
                        query = "UPDATE  Account  SET Root=@level   where IdParent =@Parent";
                        var parameter = new DynamicParameters();
                        parameter.Add("@level", level);
                        parameter.Add("@Parent", acc.Id);
                        connection.Execute(query, parameter);

                    }
                    return;
                }
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "PerbaikiLevel failed for level={level}", level);
                
            }

        }

        public async Task<int> ImportBunch(List<Account> lstAccount)
        {
            try
            {
                foreach (Account account in lstAccount)
                {
                    CreateAsync(account);

                }
                return 1;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impoert salah" );
                return 0;
            }

        }

 
        
        public async Task<int> CreateAsync(Account account)
        {
        
            try
            {
                var query = "INSERT  into Account (Id,Root,Name,IdParent,Leaf,Debet) values (@pId,@pRoot,@pNama,@pIdParent,@pLeaf,@pDebet)";
                var p = new DynamicParameters();
                p.Add("@pId", account.Id);
                p.Add("@pRoot", account.Root);
                p.Add("@pNama", account.Name);
                p.Add("@pIdParent", account.IdParent);
                p.Add("@pLeaf", account.Leaf);

                p.Add("@pDebet", account.Debet);
                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Create Account salah");
                return 0;

            }


        }
        public async  Task<int> UpdateAsync(Account account)
        {
            
            try
            {
                var query = "update Account set NAMe=@Name,idparent=@idparent, Leaf= @leaf, Root= @root where id=@ID";
                var p = new DynamicParameters();
                p.Add("@Name", account.Name);
                p.Add("@idparent", account.IdParent);
                p.Add("@leaf", account.Leaf);
                p.Add("@root", account.Root);
                p.Add("@ID", account.Id);

                using (IDbConnection connection = connectionFactory())
                {
                      return await connection.ExecuteAsync(query, p);
                }

            }
            catch (Exception exp)
            {

                _logger.LogError(exp, "Update Account gagal");
                return 0;
            }
        }




        

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                const string sql = "DELETE FROM Account WHERE Id = @Id;";
                
                var p = new DynamicParameters();
                p.Add("@Id", id);
                using (IDbConnection connection = connectionFactory())
                {
                    await connection.ExecuteAsync(sql, p);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed for Contact Id={Id}", id);
                return false;
            }
        }

        // Trouble this is cause ID in account is a string,
        // but the interface expects an int. We can implement this method to throw an exception or handle it appropriately.
        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
