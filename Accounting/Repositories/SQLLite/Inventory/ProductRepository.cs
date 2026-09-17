using Accounting.IRepositories;
using Accounting.Domain.Entities;
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
    internal class ProductRepository : IProductRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(Func<IDbConnection> connectionFactory, ILogger<ProductRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> CreateAsync(Product entity)
        {
            const string sql = @"INSERT INTO Product (Jenis,Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock)
                             VALUES (@Jenis,@Code, @Name, @PurchasePrice, @CurrentSellingPrice, @CurrentStock);
                             SELECT last_insert_rowid();";
            try
            {
                using var conn = _connectionFactory();
                var id = await conn.ExecuteScalarAsync<long>(sql, entity).ConfigureAwait(false);
                return (int)id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync Product failed for Code={Code}", entity?.Code);
                return 0;
            }
        }
       
        public async Task<int> UpdateAsync(Product entity)
        {
            const string sql = @"UPDATE Product SET Jenis=@Jenis, Code = @Code, Name = @Name, PurchasePrice = @PurchasePrice,
                             CurrentSellingPrice = @CurrentSellingPrice, CurrentStock = @CurrentStock
                             WHERE Id = @Id;";
            try
            {
                using var conn = _connectionFactory();
                return await conn.ExecuteAsync(sql, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync Product failed Id={Id}", entity?.Id);
                return 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Product WHERE Id = @Id;";
            try
            {
                using var conn = _connectionFactory();
                var affected = await conn.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync Product failed Id={Id}", id);
                return false;
            }
        }

        public async Task<int> ImportBunch(List<Product> products)
        {
            if (products == null || products.Count == 0) return 0;
            const string sql = @"INSERT INTO Product (Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock)
                             VALUES (@Code, @Name, @PurchasePrice, @CurrentSellingPrice, @CurrentStock);";
            try
            {
                using var conn = _connectionFactory();
                using var tran = conn.BeginTransaction();
                foreach (var p in products)
                {
                    await conn.ExecuteAsync(sql, p, tran).ConfigureAwait(false);
                }
                tran.Commit();
                return products.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportBunch Product failed");
                return 0;
            }

        }
    }
}
