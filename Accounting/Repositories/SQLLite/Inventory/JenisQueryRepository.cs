using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Inventory
{
    internal class JenisQueryRepository : IJenisQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public JenisQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<JenisDTO>> GetAllAsync()
        {
            const string sql = @"
SELECT j.ID,j.Kode, j.Nama, j.ParentID, p.Nama AS ParentNama
FROM Jenis j
LEFT JOIN Jenis p ON p.ID = j.ParentID
ORDER BY COALESCE(p.Nama, j.Nama), j.Nama;";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<JenisDTO>(sql).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<JenisDTO?> GetByIdAsync(int id)
        {
            const string sql = @"
SELECT j.ID,j.Kode, j.Nama, j.ParentID, p.Nama AS ParentNama
FROM Jenis j
LEFT JOIN Jenis p ON p.ID = j.ParentID
WHERE j.ID = @ID;";
            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<JenisDTO>(sql, new { ID = id }).ConfigureAwait(false);
            return row;
        }

        public async Task<List<JenisDTO>> SearchAsync(string keyword)
        {
            const string sql = @"
SELECT j.ID, j.Kode, j.Nama, j.ParentID, p.Nama AS ParentNama
FROM Jenis j
LEFT JOIN Jenis p ON p.ID = j.ParentID
WHERE j.Nama LIKE @p
ORDER BY p.Nama, j.Nama;";
            var p = $"%{keyword}%";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<JenisDTO>(sql, new { p }).ConfigureAwait(false);
            return rows.ToList();
        }
    }
}
