using Accounting.DTO;
using Accounting.IRepositories;
using Dapper;
using System.Data;


namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class SaldoAwalReadRepository :  ISaldoAwalReadRepository
    {
        Func<IDbConnection> connectionFactory;

        public SaldoAwalReadRepository(Func<IDbConnection> _connectionFactory)
        {
            this.connectionFactory = _connectionFactory;
        }


        public async Task<List<SaldoAwalDTO>> GetOnYear(int year)
    {
         try
            {
                string query = @"SELECT SALDOAWAL.Id, Year, AccountCode, Account.Name as AccountName , SALDOAWAL.Debet, SALDOAWAL.Credit
                from SALDOAWAL INNER JOIN Account ON SALDOAWAL.AccountCode=Account.ID  WHERE Year=@year ";
                query = query.Replace("\r\n", "");

                var queryParams = new
                {
                    year= year,
                    
                };
                using (IDbConnection connection = connectionFactory())
                {


                    var result = await connection.QueryAsync<SaldoAwalDTO>(
                        query,
                        param: queryParams
                    );
                    return result.ToList<SaldoAwalDTO>();
                }
                
            }
            catch (Exception exp)
            {
              //  Error = $"{exp.Message} - {exp.StackTrace}";
                return null;
            }

        }

       public async Task<SaldoAwalDTO> GetByID(int id)
       {
            try
            {
                // 1. Ambil query SQL (Typo AcountName diperbaiki menjadi AccountName jika diperlukan)
                string query = @"SELECT SALDOAWAL.Id, Year, AccountCode, Account.Name as AccountName, SALDOAWAL.Debet, SALDOAWAL.Credit
                        FROM SALDOAWAL 
                        INNER JOIN Account On trim(Account.Id) = trim(SALDOAWAL.AccountCode) 
                        WHERE SALDOAWAL.Id = @id";

                // Pembersihan \r\n sebenarnya opsional untuk SQLite, tapi jika ingin dipertahankan silakan:
                query = query.Replace("\r\n", " ");

                // 2. Deklarasi parameter Dapper cukup gunakan nama property anonim tanpa tanda '@'
                var queryParams = new { id = id };

                // 3. Gunakan _getConnection() factory untuk membuat scope connection baru yang aman di-dispose
                using (IDbConnection connection = connectionFactory())
                {
                    var result = await connection.QuerySingleOrDefaultAsync<SaldoAwalDTO>(
                        query,
                        param: queryParams
                    );

                    // 4. WAJIB mengembalikan hasil di dalam blok sukses
                    return result;
                }
            }
            catch (Exception exp)
            {
                // 5. Perbaikan penanganan error tanpa variabel 'i' yang tidak dikenal
              //  Error = $"{exp.Message} - {exp.StackTrace}";
                return null;
            }
        }


    }
}
