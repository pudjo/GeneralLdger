using Accounting.Domain.Entities;
using Accounting.IRepositories.Inventory;
using Microsoft.Extensions.Logging;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class PriceLogRepository : IPriceLogRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly ILogger<PriceLogRepository> _logger;

        public PriceLogRepository(Func<IDbConnection> connectionFactory, ILogger<PriceLogRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> CreateAsync(PriceLog entity)
        {
            const string sql = @"
INSERT INTO PriceLog (ProductId, Tanggal, HargaJual, CreatedBy)
VALUES (@ProductId, @Tanggal, @HargaJual, @CreatedBy);
SELECT last_insert_rowid();";
            try
            {
                using var conn = _connectionFactory();
                var id = await conn.ExecuteScalarAsync<long>(sql, new
                {
                    entity.ProductId,
                    entity.Tanggal,
                    entity.HargaJual,
                    entity.CreatedBy
                }).ConfigureAwait(false);
                return (int)id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create PriceLog failed for ProductId={ProductId}", entity?.ProductId);
                return 0;
            }
        }

        public async Task<int> UpdateAsync(PriceLog entity)
        {
            const string sql = @"
UPDATE PriceLog
SET ProductId = @ProductId,
    Tanggal = @Tanggal,
    HargaJual = @HargaJual,
    CreatedBy = @CreatedBy
WHERE Id = @Id;";
            try
            {
                using var conn = _connectionFactory();
                return await conn.ExecuteAsync(sql, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update PriceLog failed Id={Id}", entity?.Id);
                return 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM PriceLog WHERE Id = @Id;";
            try
            {
                using var conn = _connectionFactory();
                var affected = await conn.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete PriceLog failed Id={Id}", id);
                return false;
            }
        }

        public async Task<int> ImportBunch(List<PriceLog> list)
        {
            if (list == null || list.Count == 0) return 0;
            const string sql = @"INSERT INTO PriceLog (ProductId, Tanggal, HargaJual, CreatedBy) VALUES (@ProductId, @Tanggal, @HargaJual, @CreatedBy);";
            try
            {
                using var conn = _connectionFactory();
                using var tran = conn.BeginTransaction();
                foreach (var p in list)
                {
                    await conn.ExecuteAsync(sql, new { p.ProductId, p.Tanggal, p.HargaJual, p.CreatedBy }, tran).ConfigureAwait(false);
                }
                tran.Commit();
                return list.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportBunch PriceLog failed");
                return 0;
            }
        }

    }
}
