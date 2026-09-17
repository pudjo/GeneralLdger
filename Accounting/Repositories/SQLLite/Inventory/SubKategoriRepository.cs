using Accounting.Domain.Entities;
using Accounting.IRepositories.Inventory;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class SubKategoriRepository : ISubKategoriRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly ILogger<SubKategoriRepository> _logger;

        public SubKategoriRepository(Func<IDbConnection> connectionFactory, ILogger<SubKategoriRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> CreateAsync(SubKategori entity)
        {
            const string sql = @"INSERT INTO SubKategori (Nama, KategoriID) VALUES (@Nama, @KategoriID); SELECT last_insert_rowid();";
            try
            {
                using var conn = _connectionFactory();
                var id = await conn.ExecuteScalarAsync<long>(sql, new { entity.Nama, entity.KategoriID }).ConfigureAwait(false);
                return (int)id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create SubKategori failed for Nama={Nama}", entity?.Nama);
                return 0;
            }
        }

        public async Task<int> UpdateAsync(SubKategori entity)
        {
            const string sql = @"UPDATE SubKategori SET Nama = @Nama, KategoriID = @KategoriID WHERE ID = @ID;";
            try
            {
                using var conn = _connectionFactory();
                return await conn.ExecuteAsync(sql, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update SubKategori failed ID={ID}", entity?.ID);
                return 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM SubKategori WHERE ID = @ID;";
            try
            {
                using var conn = _connectionFactory();
                var affected = await conn.ExecuteAsync(sql, new { ID = id }).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete SubKategori failed ID={ID}", id);
                return false;
            }
        }

        public async Task<int> ImportBunch(List<SubKategori> list)
        {
            if (list == null || list.Count == 0) return 0;
            const string sql = @"INSERT INTO SubKategori (Nama, KategoriID) VALUES (@Nama, @KategoriID);";
            try
            {
                using var conn = _connectionFactory();
                using var tran = conn.BeginTransaction();
                foreach (var item in list)
                {
                    await conn.ExecuteAsync(sql, new { item.Nama, item.KategoriID }, tran).ConfigureAwait(false);
                }
                tran.Commit();
                return list.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportBunch SubKategori failed");
                return 0;
            }
        }
    }
}


