using Accounting.DTO;
using Accounting.IRepositories;
using Dapper;
using System.Collections.ObjectModel;
using System.Data;
namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class GeneralLedgerQueryRepository :  IGeneralLedgerQueryRepository
    {
        Func<IDbConnection> connectionFactory;
        public GeneralLedgerQueryRepository(Func<IDbConnection> _connectionFactory)
        {

            connectionFactory = _connectionFactory;
        }
        public async  Task<ObservableCollection<GeneralLedgerDTO>> GetGeneralLedgers(DateTime start, DateTime end, string accountCode = "")
        {
         
            try
            {
                // 1. Susun Query SQL untuk menarik data flat dari GeneralLedger dan Nama Akun
                string query = @"
    SELECT 
        GL.Id, 
        GL.JournalHeaderId,
        GL.TrxDate,
        GL.RefNo, 
        GL.Description, 
        GL.AccountCode, 
        CAST(GL.Debet AS REAL)  AS Debet,
        CAST(GL.Credit AS REAL)  AS Credit
        
    FROM GeneralLedger GL
    WHERE 1=1"; // Perhatikan penutup tanda petik di sini

                // 1. Filter Akun untuk General Ledger
                if (!string.IsNullOrEmpty(accountCode))
                {
                  
                    query += " AND REPLACE(GL.AccountCode, '.', '') LIKE '"+ accountCode + "%'";
                }

                // 2. Filter Tanggal untuk General Ledger
                query += " AND GL.TrxDate BETWEEN @start AND @end";
                // 5. Urutkan hasil akhir
                query += " ORDER BY GL.Id,GL.TrxDate ";
                string cleanQuery = System.Text.RegularExpressions.Regex.Replace(query, @"\s+", " ").Trim();
                var queryParams = new
                {
                    start = start.ToString("yyyy-MM-dd HH:mm:ss"),
                    end = end.ToString("yyyy-MM-dd HH:mm:ss"),
                    
                };

                using (IDbConnection connection = connectionFactory())
                {
                
                    var result = await connection.QueryAsync<GeneralLedgerDTO>(
                        cleanQuery,
                        param: queryParams
                    );

                    return new ObservableCollection < GeneralLedgerDTO > (result);
                }
            }
            catch (Exception exp)
            {
                // Log error jika terjadi kegagalan koneksi atau query
                // Error = $"Gagal memuat Buku Besar: {exp.Message}";
                System.Windows.MessageBox.Show($"Kesalahan mengambil data{exp.Message}");
                return null;
            }
        }
        

        

        public async Task<ObservableCollection<GeneralLedgerDTO>> GetLabaRugi(DateTime start, DateTime end, string accountCode = "")
        {
            try
            {
                // 1. Susun Query SQL untuk menarik data flat dari GeneralLedger dan Nama Akun
                string query = @"
    SELECT 
        GL.Id, 
        GL.JournalHeaderId,
        GL.TrxDate,
        GL.RefNo, 
        GL.Description, 
        GL.AccountCode, 
        CAST(GL.Debet AS REAL)  AS Debet,
        CAST(GL.Credit AS REAL)  AS Credit
        
    FROM GeneralLedger GL
    WHERE 1=1"; // Perhatikan penutup tanda petik di sini

                // 1. Filter Akun untuk General Ledger

                // 2. Filter Tanggal untuk General Ledger
                query += " AND GL.TrxDate BETWEEN @start AND @end";
                // 5. Urutkan hasil akhir
                query += " ORDER BY GL.Id,GL.TrxDate ";
                string cleanQuery = System.Text.RegularExpressions.Regex.Replace(query, @"\s+", " ").Trim();
                var queryParams = new
                {
                    start = start.ToString("yyyy-MM-dd HH:mm:ss"),
                    end = end.ToString("yyyy-MM-dd HH:mm:ss"),

                };

                using (IDbConnection connection = connectionFactory())
                {

                    var result = await connection.QueryAsync<GeneralLedgerDTO>(
                        cleanQuery,
                        param: queryParams
                    );

                    return new ObservableCollection<GeneralLedgerDTO>(result);
                }
            }
            catch (Exception exp)
            {
                // Log error jika terjadi kegagalan koneksi atau query
                // Error = $"Gagal memuat Buku Besar: {exp.Message}";
                System.Windows.MessageBox.Show($"Kesalahan mengambil data{exp.Message}");
                return null;
            }
        }

        public async  Task<ObservableCollection<LaporanDTO>> GetLaporanAsync(int tahunBerjalan)
        {
            using (IDbConnection connection = connectionFactory())
            {
            
                string query = @"
            WITH Agg_SaldoAwal AS (
    SELECT 
        SUBSTR(AccountCode, 1, 5) AS CodeL3,
        SUM(Debet) AS TotalDebet,
        SUM(Credit) AS TotalCredit
    FROM SaldoAwal
    --WHERE Year = 2026
    GROUP BY SUBSTR(AccountCode, 1, 5)
),
Agg_GL AS (
    SELECT 
        SUBSTR(AccountCode, 1, 5) AS CodeL3,
        SUM(Debet) AS TotalDebet,
        SUM(Credit) AS TotalCredit
    FROM GeneralLedger
    -- WHERE strftime('%Y', TrxDate) = '2026' -- Jika nanti query tanggal ini diaktifkan, pastikan tipe datanya sesuai
    GROUP BY SUBSTR(AccountCode, 1, 5)
)

SELECT 
    acc.Id AS AccountCode,
    acc.Name AS AccountName,
    COALESCE(sa.TotalDebet, 0) AS SaldoAwalDebet,
    COALESCE(sa.TotalCredit, 0) AS SaldoAwalKredit,
    COALESCE(gl.TotalDebet, 0) AS TahunIniDebet,
    COALESCE(gl.TotalCredit, 0) AS TahunIniKredit
FROM Account acc
LEFT JOIN Agg_SaldoAwal sa ON acc.Id = sa.CodeL3
LEFT JOIN Agg_GL gl ON acc.Id = gl.CodeL3
WHERE LENGTH(acc.Id) = 5 
ORDER BY acc.Id;;";

                var result = await connection.QueryAsync<LaporanDTO>(query, new
                {
                    Tahun = tahunBerjalan,
                    TahunStr = tahunBerjalan.ToString()
                });

                return new ObservableCollection<LaporanDTO>(result);
            }
        }

        Task<ObservableCollection<LaporanDTO>> IGeneralLedgerQueryRepository.GetLaporanAsync(int tahunBerjalan)
        {
            throw new NotImplementedException();
        }
    }
}
