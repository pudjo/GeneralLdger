using Accounting.DTO.AP;
using Accounting.DTO.AR;
using Accounting.IRepositories.AP;
using Accounting.IRepositories.AR;
using Accounting.Repositories.SQLLite.ERP;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.AP
{
    internal class PembelianRepository : IPembelianRepository

    {
        private readonly ILogger<ContactRepository> _logger;
        private readonly Func<IDbConnection> _connectionFactory;
        public PembelianRepository(Func<IDbConnection> connectionFactory, ILogger<ContactRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;



        }


        public async Task<int> CreateAsync(PembelianDTO Pembelian, IEnumerable<PembelianDetailDTO> details)
        {
            if (Pembelian is null) throw new ArgumentNullException(nameof(Pembelian));

            using var conn = _connectionFactory();

            using var tx = conn.BeginTransaction();

            try
            {
                const string insertPembelianSql =
                    "INSERT INTO Pembelian (Description, Tanggal, VendorID, StatusID) VALUES (@Description, @Tanggal, @VendorID, @StatusID); SELECT last_insert_rowid();";

                var PembelianId = await conn.ExecuteScalarAsync<long>(
                    insertPembelianSql,
                    new
                    {
                        Pembelian.Description,
                        Pembelian.Tanggal,
                        Pembelian.VendorID,
                        Pembelian.StatusID
                    },
                    tx);


                // Insert details
                if (details != null)
                {
                    const string insertDetailSql =
                        "INSERT INTO PembelianDetail (PembelianId, ProductId, Quantity, Price, SellingPrice) " +
                        "VALUES (@PembelianId, @ProductId, @Quantity, @Price, @SellingPrice);";

                    foreach (var d in details)
                    {
                        await conn.ExecuteAsync(
                            insertDetailSql,
                            new
                            {
                                PembelianId = PembelianId,
                                d.ProductId,
                                d.Quantity,
                                d.Price,
                                d.SellingPrice
                            },
                            tx);
                    }
                }

                tx.Commit();
                return (int)PembelianId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateAsync(PembelianDTO Pembelian, IEnumerable<PembelianDetailDTO> details)
        {
            if (Pembelian is null) throw new ArgumentNullException(nameof(Pembelian));
            if (Pembelian.Id <= 0) throw new ArgumentException("Pembelian.Id must be set for update", nameof(Pembelian));

            using var conn = _connectionFactory();

            using var tx = conn.BeginTransaction();

            try
            {
                const string updatePembelianSql =
                    "UPDATE Pembelian SET Description = @Description, Tanggal = @Tanggal, VendorID = @VendorID, StatusID = @StatusID WHERE Id = @Id;";

                await conn.ExecuteAsync(
                    updatePembelianSql,
                    new
                    {
                        Pembelian.Description,
                        Pembelian.Tanggal,
                        Pembelian.VendorID,
                        Pembelian.StatusID,
                        Pembelian.Id
                    },
                    tx);


                // Replace details: delete existing details and insert new ones
                const string deleteDetailsSql = "DELETE FROM PembelianDetail WHERE PembelianId = @PembelianId;";
                await conn.ExecuteAsync(deleteDetailsSql, new { PembelianId = Pembelian.Id }, tx);

                if (details != null)
                {
                    const string insertDetailSql =
                        "INSERT INTO PembelianDetail (PembelianId, ProductId, Quantity, Price, SellingPrice) " +
                        "VALUES (@PembelianId, @ProductId, @Quantity, @Price, @SellingPrice);";

                    foreach (var d in details)
                    {
                        await conn.ExecuteAsync(
                            insertDetailSql,
                            new
                            {
                                PembelianId = Pembelian.Id,
                                d.ProductId,
                                d.Quantity,
                                d.Price,
                                d.SellingPrice
                            },
                            tx);
                    }
                }

                tx.Commit();
                return Pembelian.Id;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("id must be greater than zero", nameof(id));

            using var conn = _connectionFactory();
            using var tx = conn.BeginTransaction();

            try
            {
                const string deleteDetailsSql = "DELETE FROM PembelianDetail WHERE PembelianId = @Id;";
                await conn.ExecuteAsync(deleteDetailsSql, new { Id = id }, tx);



                const string deletePembelianSql = "DELETE FROM Pembelian WHERE Id = @Id;";
                var rows = await conn.ExecuteAsync(deletePembelianSql, new { Id = id }, tx);

                tx.Commit();
                return rows > 0;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
