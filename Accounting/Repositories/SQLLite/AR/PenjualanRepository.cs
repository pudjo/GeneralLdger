using Accounting.DTO.AR;
using Accounting.IRepositories.AR;
using Accounting.Repositories.SQLLite.ERP;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.AR
{
    internal class PenjualanRepository : IPenjualanRepository

    {
        private readonly ILogger<ContactRepository> _logger;
        private readonly Func<IDbConnection> _connectionFactory;
        public PenjualanRepository(Func<IDbConnection> connectionFactory, ILogger<ContactRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;


            
        }

        
        public async Task<int> CreateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details)
        {
            if (penjualan is null) throw new ArgumentNullException(nameof(penjualan));

            using var conn = _connectionFactory();
            
            using var tx = conn.BeginTransaction();

            try
            {
                const string insertPenjualanSql =
                    "INSERT INTO Penjualan (Description, Tanggal, CutsID, StatusID) VALUES (@Description, @Tanggal, @CutsID, @StatusID); SELECT last_insert_rowid();";

                var penjualanId = await conn.ExecuteScalarAsync<long>(
                    insertPenjualanSql,
                    new
                    {
                        penjualan.Description,
                        penjualan.Tanggal,
                        penjualan.CutsID,
                        penjualan.StatusID
                    },
                    tx);

                // Insert Address if present (use PenjualanId)
                if (!string.IsNullOrEmpty(penjualan.AddressLine1) ||
                    !string.IsNullOrEmpty(penjualan.AddressLine2) ||
                    !string.IsNullOrEmpty(penjualan.City) ||
                    !string.IsNullOrEmpty(penjualan.Country) ||
                    !string.IsNullOrEmpty(penjualan.NoHP) ||
                    !string.IsNullOrEmpty(penjualan.Email))
                {
                    const string insertAddressSql =
                        "INSERT INTO PenjualanAddress (PenjualanId, AddressLine1, AddressLine2, City, State, PostalCode, Country, NoHP, Email) " +
                        "VALUES (@PenjualanId, @AddressLine1, @AddressLine2, @City, @State, @PostalCode, @Country, @NoHP, @Email);";

                    await conn.ExecuteAsync(
                        insertAddressSql,
                        new
                        {
                            PenjualanId = penjualanId,
                            penjualan.AddressLine1,
                            penjualan.AddressLine2,
                            penjualan.City,
                            penjualan.State,
                            penjualan.PostalCode,
                            penjualan.Country,
                            penjualan.NoHP,
                            penjualan.Email
                        },
                        tx);
                }

                // Insert details
                if (details != null)
                {
                    const string insertDetailSql =
                        "INSERT INTO PenjualanDetail (PenjualanId, ProductId, Quantity, Price, SellingPrice) " +
                        "VALUES (@PenjualanId, @ProductId, @Quantity, @Price, @SellingPrice);";

                    foreach (var d in details)
                    {
                        await conn.ExecuteAsync(
                            insertDetailSql,
                            new
                            {
                                PenjualanId = penjualanId,
                                d.ProductId,
                                d.Quantity,
                                d.Price,
                                d.SellingPrice
                            },
                            tx);
                    }
                }

                tx.Commit();
                return (int)penjualanId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details)
        {
            if (penjualan is null) throw new ArgumentNullException(nameof(penjualan));
            if (penjualan.Id <= 0) throw new ArgumentException("Penjualan.Id must be set for update", nameof(penjualan));

            using var conn = _connectionFactory();
            
            using var tx = conn.BeginTransaction();

            try
            {
                const string updatePenjualanSql =
                    "UPDATE Penjualan SET Description = @Description, Tanggal = @Tanggal, CutsID = @CutsID, StatusID = @StatusID WHERE Id = @Id;";

                await conn.ExecuteAsync(
                    updatePenjualanSql,
                    new
                    {
                        penjualan.Description,
                        penjualan.Tanggal,
                        penjualan.CutsID,
                        penjualan.StatusID,
                        penjualan.Id
                    },
                    tx);

                // Replace address: delete existing for this PenjualanId then insert new if provided
                const string deleteAddressSql = "DELETE FROM PenjualanAddress WHERE PenjualanId = @PenjualanId;";
                await conn.ExecuteAsync(deleteAddressSql, new { PenjualanId = penjualan.Id }, tx);

                if (!string.IsNullOrEmpty(penjualan.AddressLine1) ||
                    !string.IsNullOrEmpty(penjualan.AddressLine2) ||
                    !string.IsNullOrEmpty(penjualan.City) ||
                    !string.IsNullOrEmpty(penjualan.Country) ||
                    !string.IsNullOrEmpty(penjualan.NoHP) ||
                    !string.IsNullOrEmpty(penjualan.Email))
                {
                    const string insertAddressSql =
                        "INSERT INTO PenjualanAddress (PenjualanId, AddressLine1, AddressLine2, City, State, PostalCode, Country, NoHP, Email) " +
                        "VALUES (@PenjualanId, @AddressLine1, @AddressLine2, @City, @State, @PostalCode, @Country, @NoHP, @Email);";

                    await conn.ExecuteAsync(
                        insertAddressSql,
                        new
                        {
                            PenjualanId = penjualan.Id,
                            penjualan.AddressLine1,
                            penjualan.AddressLine2,
                            penjualan.City,
                            penjualan.State,
                            penjualan.PostalCode,
                            penjualan.Country,
                            penjualan.NoHP,
                            penjualan.Email
                        },
                        tx);
                }

                // Replace details: delete existing details and insert new ones
                const string deleteDetailsSql = "DELETE FROM PenjualanDetail WHERE PenjualanId = @PenjualanId;";
                await conn.ExecuteAsync(deleteDetailsSql, new { PenjualanId = penjualan.Id }, tx);

                if (details != null)
                {
                    const string insertDetailSql =
                        "INSERT INTO PenjualanDetail (PenjualanId, ProductId, Quantity, Price, SellingPrice) " +
                        "VALUES (@PenjualanId, @ProductId, @Quantity, @Price, @SellingPrice);";

                    foreach (var d in details)
                    {
                        await conn.ExecuteAsync(
                            insertDetailSql,
                            new
                            {
                                PenjualanId = penjualan.Id,
                                d.ProductId,
                                d.Quantity,
                                d.Price,
                                d.SellingPrice
                            },
                            tx);
                    }
                }

                tx.Commit();
                return penjualan.Id;
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
                const string deleteDetailsSql = "DELETE FROM PenjualanDetail WHERE PenjualanId = @Id;";
                await conn.ExecuteAsync(deleteDetailsSql, new { Id = id }, tx);

                const string deleteAddressSql = "DELETE FROM PenjualanAddress WHERE PenjualanId = @Id;";
                await conn.ExecuteAsync(deleteAddressSql, new { Id = id }, tx);

                const string deletePenjualanSql = "DELETE FROM Penjualan WHERE Id = @Id;";
                var rows = await conn.ExecuteAsync(deletePenjualanSql, new { Id = id }, tx);

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
