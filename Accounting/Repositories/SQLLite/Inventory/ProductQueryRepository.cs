using Accounting.DTO;
using Accounting.IRepositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class ProductQueryRepository:IProductQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public ProductQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ProductDTO>> GetAllAsync()
        {
            const string sql = @"SELECT Id, Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock FROM Product ORDER BY Name;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<ProductDTO>(sql).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            const string sql = @"SELECT Id, Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock FROM Product WHERE Id = @Id;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<ProductDTO>(sql, new { Id = id }).ConfigureAwait(false);
            return row;
        }

        public async Task<ProductDTO?> GetByCodeAsync(string code)
        {
            const string sql = @"SELECT Id, Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock FROM Product WHERE Code = @Code;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<ProductDTO>(sql, new { Code = code }).ConfigureAwait(false);
            return row;
        }

        public async Task<List<ProductDTO>> SearchAsync(string keyword)
        {
            const string sql = @"SELECT Id, Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock FROM Product
                             WHERE Name LIKE @p OR Code LIKE @p
                             ORDER BY Name;";
            var p = $"%{keyword}%";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<ProductDTO>(sql, new { p }).ConfigureAwait(false);
            return rows.ToList();
        }
        public async Task<List<ProductDTO>> GetByJenisAsync(int jenisId)
        {
            const string sql = @"SELECT Id, Code, Name, PurchasePrice, CurrentSellingPrice, CurrentStock FROM Product
                                 WHERE Jenis = @Jenis
                                 ORDER BY Name;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<ProductDTO>(sql, new { Jenis = jenisId }).ConfigureAwait(false);
            return rows.ToList();
        }

    }
}
