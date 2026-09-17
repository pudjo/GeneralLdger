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
    internal class JenisRepository : IJenisRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly ILogger<JenisRepository> _logger;

        public JenisRepository(Func<IDbConnection> connectionFactory, ILogger<JenisRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }
        public async Task<int> CreateAsync(Jenis entity)
        {
            const string sql = @"INSERT INTO Jenis (Nama,Kode, ParentID) VALUES (@Nama,@Kode, @ParentID); SELECT last_insert_rowid();";
            try
            {
                using var conn = _connectionFactory();
                var id = await conn.ExecuteScalarAsync<long>(sql, new { entity.Nama, entity.Kode, entity.ParentID }).ConfigureAwait(false);
                return (int)id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Jenis failed for Nama={Nama} {ex.Message}", entity?.Nama);
                return 0;
            }
        }

        public async Task<int> UpdateAsync(Jenis entity)
        {
            const string sql = @"UPDATE Jenis SET Nama = @Nama,Kode=@Kode,  ParentID = @ParentID WHERE ID = @ID;";
            try
            {
                using var conn = _connectionFactory();
                return await conn.ExecuteAsync(sql, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Jenis failed ID={ID}", entity?.ID);
                return 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Jenis WHERE ID = @ID;";
            try
            {
                using var conn = _connectionFactory();
                var affected = await conn.ExecuteAsync(sql, new { ID = id }).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Jenis failed ID={ID}", id);
                return false;
            }
        }

        public async Task<int> ImportBunch(List<Jenis> list)
        {
            if (list == null || list.Count == 0) return 0;
            const string sql = @"INSERT INTO Jenis (Nama, ParentID) VALUES (@Nama, @ParentID);";
            try
            {
                using var conn = _connectionFactory();
                using var tran = conn.BeginTransaction();
                foreach (var item in list)
                {
                    await conn.ExecuteAsync(sql, new { item.Nama, item.ParentID }, tran).ConfigureAwait(false);
                }
                tran.Commit();
                return list.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportBunch Jenis failed");
                return 0;
            }
        }
    }
}

