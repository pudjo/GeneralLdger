using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Dapper;
using System.Data;


namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class JournalReadRepository :  IJournalReadRepository
    {
        Func<IDbConnection> connectionFactory;
        public JournalReadRepository(Func<IDbConnection> _connectionFactory) 
        {
            connectionFactory = _connectionFactory;
        }


        public async Task<List<JurnalDTO>> GetJurnalAndDetail(DateTime start,
            DateTime end, int id = 0)
        {
            int i = 0;
            try
            {
                string query = @"SELECT H.Id, H.Refno, H.JournalDate, H.Description, cast(J.Debet as Real)  as TotalDebet,
                cast(J.Credit  as real) as TotalCredit, D.ID as IDDetail, D.AccountCode, cast(D.Credit as real) as Credit , cast(D.Debet as Real) as Debet, 
                ac.Name as AccountName FROM Jurnal H INNER JOIN JournalDetail D ON H.Id = D.JournalHeaderId  
                inner join Account ac on Trim(D.AccountCode)= trim(ac.ID) 
                inner join (Select JournalHeaderId, Sum(cast(Debet * 1.0 as real)) as Debet
                ,sum(cast(credit * 1.0 as real)) as Credit from JournalDetail group by JournalHeaderId)  J 
                ON J.JournalHeaderId= H.ID
                where H.JournalDate between @start and @end";

                if (id > 0)
                {
                    query = query + " AND Jurnal.ID=@id";
                }
                //ORDER BY H.JournalDate, H.Id, D.ID";

                query = query.Replace("\r\n", "");
                var jurnalDictionary = new Dictionary<int, JurnalDTO>();
                var queryParams = new
                {
                    start = start.ToString("yyyy-MM-dd HH:mm:ss"),
                    end = end.ToString("yyyy-MM-dd HH:mm:ss"),
                    id = id
                };
                using (IDbConnection connection = connectionFactory())
                {
                    var result = await connection.QueryAsync<JurnalDTO, JournalDetailDTO, JurnalDTO>(
                     
                        
                        query, (header, detail) =>
                        {
                            i++;
                            // 1. Cek atau Buat Header baru
                            if (!jurnalDictionary.TryGetValue(header.Id, out var existingHeader))
                            {
                                existingHeader = header;
                                jurnalDictionary.Add(existingHeader.Id, existingHeader);
                            }

                            // Tambahkan Detail (pastikan Detail tidak null dari JOIN)
                            if (detail != null)
                            {
                                existingHeader.Detail.Add(detail);
                            }

                            return existingHeader;
                        },
                        param: queryParams,
                    splitOn: "AccountCode" // Kolom pertama di tabel Detail
                 );
                    return jurnalDictionary.Values.ToList();

                }

            }
            catch (Exception exp)
            {
              //  Error = $"{exp.Message}  pada baris {i}";
                return null;
            }
        }
        public async Task<Jurnal> GetByID(int id)
        {
            try
            {
                var query = "SELECT *  from Jurnal  Where Id= @id ";
                using (IDbConnection connection = connectionFactory())
                {
                    var jurnal = await connection.QuerySingleAsync<Jurnal>(query, new { id = id });
                    return jurnal;
                }
                

            }
            catch (Exception exp)
            {
                //                Error = exp.Message;
                return null;
            }
        }
    }
}
