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
    internal class KategoriQueryRepository : IKategoriQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public KategoriQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<KategoriDTO>> GetAllAsync()
        {
            const string sql = @"SELECT ID, Nama FROM Kategori ORDER BY Nama;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<KategoriDTO>(sql).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<KategoriDTO?> GetByIdAsync(int id)
        {
            const string sql = @"SELECT ID, Nama FROM Kategori WHERE ID = @ID;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<KategoriDTO>(sql, new { ID = id }).ConfigureAwait(false);
            return row;
        }

        public async Task<KategoriDTO?> GetByNameAsync(string name)
        {
            const string sql = @"SELECT ID, Nama FROM Kategori WHERE Nama = @Nama LIMIT 1;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<KategoriDTO>(sql, new { Nama = name }).ConfigureAwait(false);
            return row;
        }

        public async Task<List<KategoriDTO>> SearchAsync(string keyword)
        {
            const string sql = @"SELECT ID, Nama FROM Kategori WHERE Nama LIKE @p ORDER BY Nama;";
            var p = $"%{keyword}%";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<KategoriDTO>(sql, new { p }).ConfigureAwait(false);
            return rows.ToList();
        }
    }

}

