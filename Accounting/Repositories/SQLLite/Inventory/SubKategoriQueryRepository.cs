using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using System;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class SubKategoriQueryRepository : ISubKategoriQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public SubKategoriQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<SubKategoriDTO>> GetAllAsync()
        {
            const string sql = @"
SELECT s.ID, s.Nama, s.KategoriID, k.Nama as KategoriNama
FROM SubKategori s
LEFT JOIN Kategori k ON k.ID = s.KategoriID
ORDER BY k.Nama, s.Nama;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<SubKategoriDTO>(sql).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<SubKategoriDTO?> GetByIdAsync(int id)
        {
            const string sql = @"
SELECT s.ID, s.Nama, s.KategoriID, k.Nama as KategoriNama
FROM SubKategori s
LEFT JOIN Kategori k ON k.ID = s.KategoriID
WHERE s.ID = @ID;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<SubKategoriDTO>(sql, new { ID = id }).ConfigureAwait(false);
            return row;
        }

        public async Task<List<SubKategoriDTO>> GetByKategoriIdAsync(int kategoriId)
        {
            const string sql = @"
SELECT s.ID, s.Nama, s.KategoriID, k.Nama as KategoriNama
FROM SubKategori s
LEFT JOIN Kategori k ON k.ID = s.KategoriID
WHERE s.KategoriID = @KategoriID
ORDER BY s.Nama;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<SubKategoriDTO>(sql, new { KategoriID = kategoriId }).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<List<SubKategoriDTO>> SearchAsync(string keyword)
        {
            const string sql = @"
SELECT s.ID, s.Nama, s.KategoriID, k.Nama as KategoriNama
FROM SubKategori s
LEFT JOIN Kategori k ON k.ID = s.KategoriID
WHERE s.Nama LIKE @p OR k.Nama LIKE @p
ORDER BY k.Nama, s.Nama;";
            var p = $"%{keyword}%";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<SubKategoriDTO>(sql, new { p }).ConfigureAwait(false);
            return rows.ToList();
        }
    }
}

