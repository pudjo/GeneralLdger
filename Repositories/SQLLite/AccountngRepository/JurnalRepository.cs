

using Accounting.Domain;
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Views.JurnalView;
using ControlzEx.Standard;
using Dapper;
using System.Data;
using System.Transactions;

namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class JurnalRepository : BaseRepository, IJournalRepository
    {

        private readonly Func<IDbConnection> _getConnection;
        private readonly Func<IDbTransaction> _getTransaction;

        public JurnalRepository(Func<IDbConnection> connectionFactory, Func<IDbTransaction> transactionFactory)
            : base(connectionFactory, transactionFactory)
        {

            _getConnection = connectionFactory;
            _getTransaction = transactionFactory;

            // Konstruktor ini boleh kosong karena datanya langsung dioper ke : base()
        }
        public async Task<int> CreateBunc( List<GeneralLedger> listGL )
        {
            
            int lastID = 0;
            string queryJurnalImport = @"INSERT  into JurnalImport (RefNo,AccountCode,Tanggal,Description,Debit,Credit ) values 
                             (@RefNo,@AccountCode,@Tanggal,@Description,@Debit,@Credit)";  

            string query = @"INSERT  into Jurnal (RefNo,JournalDate,Description,Status,Source) values 
                             (@RefNo,@JournalDate,@Description,@Status,@Source);  
                         
                            SELECT CAST(last_insert_rowid() AS INT);";

            var sql = @"INSERT INTO JournalDetail (JournalHeaderId, AccountCode, Debet, Credit) VALUES (@JournalHeaderId, @AccountCode, @Debet, @Credit);";
            var sqlGL = @"INSERT INTO GeneralLedger (JournalHeaderId,RefNo,TrxDate,Description,Debet,
                              Credit,AccountCode,Status,Source) values (@JournalHeaderId,@RefNo,@TrxDate,@Description,@Debet,
                              @Credit,@AccountCode,@Status,@Source)";

           
            
            List<Jurnal> listjurnal = new List<Jurnal>();
            var groupedData = listGL.GroupBy(x => x.RefNo ).ToList();


            //mendefinisikan listjurnal baru berdasarkan grup RefNo dari listGL
            foreach (var group in groupedData)
            {
                // Jika user menghapus NoBukti di grid sampai kosong, abaikan sementara dari header
                if (string.IsNullOrWhiteSpace(group.Key)) continue;

                var firstRow = group.First();
                // Tambahkan ke listHeader baru
                listjurnal.Add(new Jurnal
                {
                    RefNo = group.Key,                     // Ini akan otomatis menjadi NoBukti yang baru
                    JournalDate = firstRow.TrxDate,
                    Description = firstRow.Description,
                    Status = 0,  // Otomatis menjumlahkan ulang item yang tersisa di grup ini
                    Source = 0
                });
            }



            var connection = _getConnection();
            var transaction = _getTransaction();
            
            
           
                

                if (listjurnal == null)
                {
                    System.Windows.MessageBox.Show("Peringatan: listjurnal nilainya NULL!");
                    return 0;
                }
                try
                {
                    int i = 0;
                    foreach (Jurnal jurnal in listjurnal)
                    {

                        Console.WriteLine($" ini jurnal baris ke {++i}");

                        lastID = await connection.ExecuteScalarAsync<int>(query,
                         new
                         {
                             jurnal.RefNo,
                             jurnal.JournalDate,
                             jurnal.Description,
                             jurnal.Status,
                             jurnal.Source
                         }, transaction);

                        // 1. Definisikan list parameter yang akan dikirim (menggabungkan lastID)
                        var detailParameters = listGL.Where(gl => gl.RefNo?.Trim() == jurnal.RefNo?.Trim()).Select(jd => new
                        {
                            JournalHeaderId = lastID, // Gunakan lastID untuk parameter @Id
                            jd.AccountCode,
                            jd.Debet,
                            jd.Credit  //@JournalHeaderId, @AccountCode, @Debet, @Credit
                        }).ToList();
                        if (detailParameters.Any())
                        {
                            await connection.ExecuteAsync(sql, detailParameters, transaction);
                        }
                        var detailGL = listGL.Where(gl => gl.RefNo?.Trim() == jurnal.RefNo?.Trim()).Select(gl => new
                        {
                            JournalHeaderId = lastID, // Gunakan lastID untuk parameter @Id
                            gl.RefNo,
                            gl.TrxDate,
                            gl.Description,
                            gl.Debet,
                            gl.Credit,
                            gl.AccountCode,
                            Status = jurnal.Status,
                            Source = jurnal.Source
                        }).ToList();
                        if (detailGL.Any())
                        {
                            await connection.ExecuteAsync(sqlGL, detailGL, transaction);
                        }
                    }
                    // System.Windows.MessageBox.Show($"Selesai memproses loop. Total sukses: {i}");
                        System.Windows.MessageBox.Show($"Memproses {i}");
                return listGL.Count;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"TERJADI ERROR DI DALAM LOOP:{ex.Message}Stack Trace:{ex.StackTrace}");
                return  0;
            }
        }
        public async Task<bool> DeleteBunc(List<JurnalDTO> listjurnal)
        {
            if (listjurnal == null)
            {
                System.Windows.MessageBox.Show("Peringatan: listjurnal nilainya NULL!");
                return false;

            }

            var connection = _getConnection();
            var transaction = _getTransaction();

            bool lastID = true;
            string queryHeader = @"DELETE FROM Jurnal where ID=@ID ";
            string queryDetail = @"DELETE FROM JournalDetail where JournalHeaderId=@JournalHeaderId";
            string queryGL = @"DELETE FROM GeneralLedger where JournalHeaderId=@JournalHeaderId";

            try
            {
                int i = 0;
                foreach (JurnalDTO jurnal in listjurnal)
                {
                    Console.WriteLine($" ini jurnal baris ke {++i}");

                    // 1. HAPUS DETAIL DULU (Agar tidak melanggar Foreign Key Constraint)
                    // Definisikan nama properti secara eksplisit (JournalHeaderId = jurnal.Id)
                    await connection.ExecuteAsync(queryDetail, new { JournalHeaderId = jurnal.Id }, transaction);

                    // 2. HAPUS GL
                    await connection.ExecuteAsync(queryGL, new { JournalHeaderId = jurnal.Id }, transaction);

                    // 3. HAPUS HEADER (Gunakan ExecuteAsync, bukan ExecuteScalarAsync)
                    await connection.ExecuteAsync(queryHeader, new { ID = jurnal.Id }, transaction);
                }

                System.Windows.MessageBox.Show($"Berhasil memproses {i} data.");
            }
            catch (Exception ex)
            {
                // Tambahkan blok catch untuk menangani error dan mencegah aplikasi crash tiba-tiba
                System.Windows.MessageBox.Show($"Terjadi kesalahan saat menghapus data: {ex.Message}",
                                                "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                // Opsi: lakukan transaction.Rollback(); jika diperlukan di sini
            }
            return lastID;


        }


        public async Task<int> CreateAsync(Jurnal entity)
        {

            var connection = _getConnection();
            var transaction = _getTransaction();

            int lastID = 0;
            string query = @"INSERT  into Jurnal (RefNo,JournalDate,Description,Status,Source) values 
                    (@RefNo,@JournalDate,@Description,@Status,@Source);        
                    SELECT CAST(last_insert_rowid() AS INT);";


            lastID = await connection.ExecuteScalarAsync<int>(query,
            new
            {
                entity.RefNo,
                entity.JournalDate,
                entity.Description,
                entity.Status,
                Source = 0,
            }, transaction);


            // 1. Definisikan list parameter yang akan dikirim (menggabungkan lastID)
            var detailParameters = entity.Detail.Where(x => x.AccountCode != null).Select(jd => new
            {
                JournalHeaderId = lastID, // Gunakan lastID untuk parameter @Id
                jd.AccountCode,
                jd.Debet,
                jd.Credit
            }).ToList();

            // Query tetap sama
            var sqlDetail = @"INSERT INTO JournalDetail (JournalHeaderId, AccountCode, Debet, Credit) VALUES (@JournalHeaderId, @AccountCode, @Debet, @Credit);";

            if (detailParameters.Any())
            {
                await connection.ExecuteAsync(sqlDetail, detailParameters, transaction);


                List<GeneralLedger> generalLedgers = entity.Detail.Where(x => x.AccountCode != null)
                                      .Select(jd => new GeneralLedger
                                      {
                                          // 1. Data Diambil dari Header (Objek 'jurnal' atau variabel lokal)
                                          JournalHeaderId = lastID,                       // Menggunakan ID Header yang baru disimpan
                                          RefNo = entity.RefNo,              // Nomor referensi dari header jurnal
                                          TrxDate = entity.JournalDate,          // Tanggal transaksi dari header jurnal
                                          Description = entity.Description,  // Keterangan dari header jurnal
                                          AccountCode = jd.AccountCode,      // Kode akun dari baris detail
                                          Debet = jd.Debet,                  // Nilai debet dari baris detail
                                          Credit = jd.Credit,                 // Nilai kredit dari baris detail
                                          Status = entity.Status,            // Status dari header jurnal
                                          Source = AppSession.CurrentUser.Id,            // Source dari header jurnal
                                      }).ToList();
                if (generalLedgers.Count == detailParameters.Count)
                {
                    try
                    {
                        string sqlgl = @"INSERT INTO GeneralLedger (JournalHeaderId, 
                                      RefNo, TrxDate, Description, AccountCode, Debet, Credit,  Status, Source) 
                                      VALUES (@JournalHeaderId, @RefNo, @TrxDate, @Description, @AccountCode,@Debet, @Credit,  @Status, @Source)";

                        await connection.ExecuteAsync(sqlgl, generalLedgers, transaction); // Dapper otomatis membaca properti dari list objek
                    }  catch (Exception ex)
                    {
                        Error = ex.Message;

                    }
                }
                else
                    return 0;
            }
                return lastID;
        }
          
        

        public async Task<int> UpdateAsync(Jurnal entity)
        {
            try
            {
                int lastID = 0;
                string query = @"UPDATE Jurnal SET RefNo=@RefNo,JournalDate=@JournalDate,Description= @Description WHERE Id= @Id ";
                
                
                    using (var trans =
                         Connection.BeginTransaction())
                    {
                        try
                        {

                            lastID = await Connection.ExecuteScalarAsync<int>(query,
                            new
                            {
                                entity.RefNo,
                                entity.JournalDate,
                                entity.Description,
                                entity.Id
                            }, trans);

                            string queryDelete = @"DELETE JournalDetail  WHERE JournalHeaderId= @Id ";
                            await Connection.ExecuteScalarAsync<int>(queryDelete, new { Id = entity.Id }, trans);

                            // 1. Definisikan list parameter yang akan dikirim (menggabungkan lastID)
                            var detailParameters = entity.Detail.Select(jd => new
                            {
                                journalHeaderId = entity.Id, // Gunakan lastID untuk parameter @Id
                                jd.AccountCode,
                                jd.Debet,
                                jd.Credit
                            }).ToList();

                            // 2. Query tetap sama
                            var sql = @"INSERT INTO JournalDetail (JournalHeaderId, AccountCode, Debet, Credit) VALUES (@JournalHeaderId, @AccountCode, @Debet, @Credit);";
                            await Connection.ExecuteAsync(sql, detailParameters, trans);

                            trans.Commit();
                            lastID = entity.Id;
                            
                        }
                        catch (Exception ex)
                        {

                       //     Error = ex.Message;
                            trans.Rollback();
                            lastID = 0;
                        }
                        return lastID;

                    }
                
            }
            catch (Exception exp)
            {
                return 0;
            }
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }



        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

