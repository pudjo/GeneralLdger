using Accounting.Domain.Entities;
using Accounting.IRepositories;

using Dapper;
using Microsoft.Extensions.Logging;

using System.Data;

using static Dapper.SqlMapper;

namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class WriteSaldoAwalRepository : BaseRepository, IWriteSaldoAwalRepository
    {
        private readonly ILogger<SaldoAwalReadRepository> _logger;
        private readonly Func<IDbConnection> _getConnection;
        private readonly Func<IDbTransaction> _getTransaction;

        public WriteSaldoAwalRepository(
                Func<IDbConnection> getConnection, Func<IDbTransaction> getTransaction)
        : base(getConnection, getTransaction) 
        {
            
            _getConnection = getConnection;
            _getTransaction = getTransaction;
        }
       
        public async Task<int> CreateAsync(SaldoAwal entity)
        {
            try
            {
                var connection = _getConnection();
                var transaction = _getTransaction();

                var query = "INSERT  into SaldoAwal (Year,AccountCode,Debet,Credit) values (@Year,@AccountCode,@Debet,@Credit)";
                var p = new DynamicParameters();
                
                p.Add("@Year", entity.Year);
                p.Add("@AccountCode", entity.AccountCode);
                p.Add("@Debet", entity.Debet);
                p.Add("@Credit", entity.Credit);
                
                
                return await connection.ExecuteAsync(query, p, transaction: transaction);
                
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return 0;

            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var query = "DLEETE SaldoAwal where ID=@ID ";
                var p = new DynamicParameters();

                p.Add("@ID", id);

                using (var connection = Connection)
                {
                    int ret = await connection.ExecuteAsync(query, p);
                    return ret > 0; 
                }
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return false ;

            }
        }
 

        public async Task<int> UpdateAsync(SaldoAwal entity)
        {
            try
            {
                var connection = _getConnection();
                var transaction = _getTransaction();

                var query = "UPDATE SaldoAwal SET Debet=@Debet,Credit=@Credit " +
                            " WHERE Year=@Year and AccountCode=@AccountCode AND ID=@ID";
                var p = new DynamicParameters();

                p.Add("@Debet", entity.Debet);
                p.Add("@Credit", entity.Credit);
                p.Add("@Year", entity.Year);
                p.Add("@AccountCode", entity.AccountCode);
                p.Add("@ID", entity.Id);

                
                
                return await connection.ExecuteAsync(query, p, transaction: transaction);
                
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return 0;

            }
        }
    }
}
