using Accounting.Domain.Entities;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite.Master;
using Dapper;


using Microsoft.Extensions.Logging;
using System.Data;
using System.Xml.Linq;

namespace Accounting.Repositories.SQLLite.Administration
{
    internal class UserRepository :  IUserRepository
    {
        private readonly ILogger<AccountRepository> _logger;
        Func<IDbConnection> connectionFactory;

        public UserRepository(Func<IDbConnection> _connectionFactory
        ,ILogger<AccountRepository> logger)
        {
            connectionFactory= _connectionFactory;

            _logger = logger;
        }


        public async Task<int> CreateAsync(User entity)
        {
            try
            {
                var query = "INSERT  into User (UserID,Name,Email,HandPhoneNumber, Status) values (@UserID,@Name,@Email,@HandPhoneNumber, @Status)";
                var p = new DynamicParameters();

                
                p.Add("@UserID", entity.UserID);
                p.Add("@Name", entity.Name);
                p.Add("@Email", entity.Email);
                p.Add("@HandPhoneNumber", entity.HandPhoneNumber);
                p.Add("@Status", entity.Status);

                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                return 0;

            }


        }

        public async Task<bool> DeleteAsync(int  id)
        {
          
        
            try
            {
                var query = "UPDATE User set Status=9 WHERE ID=@Id";
                var p = new DynamicParameters();

                p.Add("@Id", id );

                using (IDbConnection connection = connectionFactory())
                {
                    int hasil = await connection.ExecuteAsync(query, p);
                    return hasil > 0;
                }
            }
            catch (Exception exp)
            {

                return false;

            }


        }

        public async Task<List<User>> GetAllAsync()
        {
            try
            {

                var query = "SELECT * from User where Status<9 order by User.Name";
                using (IDbConnection connection = connectionFactory())

                {
                    var lstUser = await connection.QueryAsync<User>(query).ConfigureAwait(false);
                    return lstUser.ToList();

                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }
        public async Task<List<User>> GetAllAsyncTest()
        {
            try
            {


                List<User> sampleUsers = new List<User>
{
    new User { Id = 1, UserID = "USR001", Name = "Ahmad Hidayat", Email = "ahmad.hidayat@example.com", Status = 1, HandPhoneNumber = "081234567890", Password = "Password123" },
    new User { Id = 2, UserID = "USR002", Name = "Budi Santoso", Email = "budi.santoso@example.com", Status = 1, HandPhoneNumber = "082345678901", Password = "Password123" },
    new User { Id = 3, UserID = "USR003", Name = "Citra Lestari", Email = "citra.lestari@example.com", Status = 1, HandPhoneNumber = "083456789012", Password = "Password123" },
    new User { Id = 4, UserID = "USR004", Name = "Dewi Saputri", Email = "dewi.saputri@example.com", Status = 0, HandPhoneNumber = "084567890123", Password = "Password123" }, // Status 0 (Non-Aktif)
    new User { Id = 5, UserID = "USR005", Name = "Eko Prasetyo", Email = "eko.prasetyo@example.com", Status = 1, HandPhoneNumber = "085678901234", Password = "Password123" },
    new User { Id = 6, UserID = "USR006", Name = "Fitriani", Email = "fitriani@example.com", Status = 1, HandPhoneNumber = "086789012345", Password = "Password123" },
    new User { Id = 7, UserID = "USR007", Name = "Gilang Ramadhan", Email = "gilang.r@example.com", Status = 1, HandPhoneNumber = "087890123456", Password = "Password123" },
    new User { Id = 8, UserID = "USR008", Name = "Hendra Wijaya", Email = "hendra.w@example.com", Status = 0, HandPhoneNumber = "088901234567", Password = "Password123" }, // Status 0 (Non-Aktif)
    new User { Id = 9, UserID = "USR009", Name = "Indah Permatasari", Email = "indah.p@example.com", Status = 1, HandPhoneNumber = "089012345678", Password = "Password123" },
    new User { Id = 10, UserID = "USR010", Name = "Joko Susilo", Email = "joko.susilo@example.com", Status = 1, HandPhoneNumber = "081122334455", Password = "Password123" }
};
                return sampleUsers;
            }
            catch (Exception exp)
            {
                return null;

            }
        }

        public async Task<User> GetByIdAsync(string id)
        {
            try
            {

                var query = "SELECT * from User where Id= Id = @Id";
                var p = new DynamicParameters();
                using (IDbConnection connection = connectionFactory())
                {
                    var user= connection.QuerySingle<User>(query, new { Id = id });
                    return user;
                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }

        public async Task<int> UpdateAsync(User entity)
        {
            try
            {
                var query = "UPDATE User set UserID=@UserID, Name=@Name,Email=@Email,HandPhoneNumber=@HandPhoneNumber, " +
                    " Status=@Status,Password =@Password WHERE ID=@Id";
                var p = new DynamicParameters();
                
                    p.Add("@UserID", entity.UserID);
                p.Add("@Name", entity.Name);
                p.Add("@Email", entity.Email);
                p.Add("@HandPhoneNumber", entity.HandPhoneNumber);
                p.Add("@Status", entity.Status);
                p.Add("@Password", entity.Password);
                p.Add("@Id", entity.Id);

                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                
                return 0;

            }

        }
    }
}

/*
 * 
 *    private readonly ILogger<AccountRepository> _logger;
        public AccountRepository(ILogger<AccountRepository> logger)
        {
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
                using (var connection = CreateConnection())
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
                throw new Exception(exp.Message, exp);
            }

        }

        public async Task<int> ImportBunch(List<Account> lstAccount)
        {
            try
            {
                foreach (Account account in lstAccount)
                {
                    Simpan(account);

                }
                return 1;

            }
            catch (Exception ex)
            {
                Error = $"Error: {ex.Message}";
                return 0;
            }

        }

        public async Task<Account> GetByIdAsync(string Id)
        {
            try
            { 
        var query = "SELECT * FROM  Account WHERE Id = @Id";
                var p = new DynamicParameters();
                using (var connection = CreateConnection())
                {
                    var account = connection.QuerySingle<Account>(query, new { Id = Id });
                    return account;
                }
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return null;
            }
        }
        public async Task<int> Simpan(Account paccount)
        {
            try
            {
                Account account = new Account();
                account = await GetByIdAsync(paccount.Id);
                if (account == null)
                {
                    return await CreateAsync(paccount);
                }
                else
                {
                    return await UpdateAsync(paccount);
                }

            }
            catch (Exception exp)
            {
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
                using (var connection = CreateConnection())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                Error = exp.Message;
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

                using (var connection = CreateConnection())
                {
                      return await connection.ExecuteAsync(query, p);
                }

            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return 0;
            }
        }

        
        public async Task<List<Account>> GetAllAsync()
        {
            try
            {
             
                var query = "SELECT * from Account order by Account.ID";

                using (var connection = CreateConnection())
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
        public async Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

 * 
 * */