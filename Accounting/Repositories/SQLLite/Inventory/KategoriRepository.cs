using Accounting.Domain.Entities;
using Accounting.IRepositories.Inventory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class KategoriRepository : IKategoriRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly ILogger<KategoriRepository> _logger;

        public KategoriRepository(Func<IDbConnection> connectionFactory, ILogger<KategoriRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> CreateAsync(Kategori entity)
        {
            const string sql = @"INSERT INTO Kategori (Nama) VALUES (@Nama); SELECT last_insert_rowid();";
            try
            {
                using var conn = _connectionFactory();
                var id = await conn.ExecuteScalarAsync<long>(sql, new { entity.Nama }).ConfigureAwait(false);
                return (int)id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Kategori failed for Nama={Nama}", entity?.Nama);
                return 0;
            }
        }

        public async Task<int> UpdateAsync(Kategori entity)
        {
            const string sql = @"UPDATE Kategori SET Nama = @Nama WHERE ID = @ID;";
            try
            {
                using var conn = _connectionFactory();
                return await conn.ExecuteAsync(sql, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Kategori failed ID={ID}", entity?.ID);
                return 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Kategori WHERE ID = @ID;";
            try
            {
                using var conn = _connectionFactory();
                var affected = await conn.ExecuteAsync(sql, new { ID = id }).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Kategori failed ID={ID}", id);
                return false;
            }
        }

        public async Task<int> ImportBunch(List<Kategori> list)
        {
            if (list == null || list.Count == 0) return 0;
            const string sql = @"INSERT INTO Kategori (Nama) VALUES (@Nama);";
            try
            {
                using var conn = _connectionFactory();
                using var tran = conn.BeginTransaction();
                foreach (var k in list)
                {
                    await conn.ExecuteAsync(sql, new { k.Nama }, tran).ConfigureAwait(false);
                }
                tran.Commit();
                return list.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportBunch Kategori failed");
                return 0;
            }
        }

    }
}
