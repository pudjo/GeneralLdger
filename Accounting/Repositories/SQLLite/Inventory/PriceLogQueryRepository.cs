using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class PriceLogQueryRepository : IPriceLogQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public PriceLogQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<PriceLogDTO>> GetAllAsync()
        {
            const string sql = @"
SELECT pl.Id, pl.ProductId, p.Name as NamaProduk, pl.Tanggal, pl.HargaJual, pl.CreatedBy
FROM PriceLog pl
LEFT JOIN Product p ON p.Id = pl.ProductId
ORDER BY pl.Tanggal DESC;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<PriceLogDTO>(sql).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<List<PriceLogDTO>> GetByProductIdAsync(int productId)
        {
            const string sql = @"
SELECT pl.Id, pl.ProductId, p.Name as NamaProduk, pl.Tanggal, pl.HargaJual, pl.CreatedBy
FROM PriceLog pl
LEFT JOIN Product p ON p.Id = pl.ProductId
WHERE pl.ProductId = @ProductId
ORDER BY pl.Tanggal DESC;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<PriceLogDTO>(sql, new { ProductId = productId }).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<PriceLogDTO?> GetLatestByProductIdAsync(int productId)
        {
            const string sql = @"
SELECT pl.Id, pl.ProductId, p.Name as NamaProduk, pl.Tanggal, pl.HargaJual, pl.CreatedBy
FROM PriceLog pl
LEFT JOIN Product p ON p.Id = pl.ProductId
WHERE pl.ProductId = @ProductId
ORDER BY pl.Tanggal DESC
LIMIT 1;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<PriceLogDTO>(sql, new { ProductId = productId }).ConfigureAwait(false);
            return row;
        }

    }
}
