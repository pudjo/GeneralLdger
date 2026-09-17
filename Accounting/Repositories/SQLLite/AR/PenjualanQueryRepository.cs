using Accounting.DTO.AR;
using Accounting.IRepositories.AR;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.AR
{
    internal class PenjualanQueryRepository : IPenjualanQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public PenjualanQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<List<PenjualanDTO>> GetAllAsync()
        {
            const string sql = @"
SELECT
    p.Id,
    p.Description,
    p.Tanggal,
    p.CutsID,
    p.StatusID,
    pa.Id AS AddressId,
    pa.AddressLine1,
    pa.AddressLine2,
    pa.City,
    pa.State,
    pa.PostalCode,
    pa.Country,
    pa.NoHP,
    pa.Email,
    c.Name AS CUstomerName
FROM Penjualan p
LEFT JOIN PenjualanAddress pa ON pa.PenjualanId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
ORDER BY p.Tanggal DESC;";

            using var conn = _connectionFactory();
            var masters = (await conn.QueryAsync<PenjualanDTO>(sql).ConfigureAwait(false)).ToList();

            if (masters.Count == 0) return masters;

            var ids = masters.Select(m => m.Id).ToArray();
            var details = await GetDetailsByPenjualanIds(conn, ids).ConfigureAwait(false);

            // assign details to masters
            var detailsLookup = details.GroupBy(d => d.PenjualanId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var m in masters)
            {
                detailsLookup.TryGetValue(m.Id, out var list);
                m.Details = list ?? new List<PenjualanDetailDTO>();
            }

            return masters;
        }

        public async Task<PenjualanDTO?> GetByIdAsync(int id)
        {
            const string sql = @"
SELECT
    p.Id,
    p.Description,
    p.Tanggal,
    p.CutsID,
    p.StatusID,
    pa.Id AS AddressId,
    pa.AddressLine1,
    pa.AddressLine2,
    pa.City,
    pa.State,
    pa.PostalCode,
    pa.Country,
    pa.NoHP,
    pa.Email,
    c.Name AS CUstomerName
FROM Penjualan p
LEFT JOIN PenjualanAddress pa ON pa.PenjualanId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
WHERE p.Id = @Id;";

            using var conn = _connectionFactory();
            var master = await conn.QuerySingleOrDefaultAsync<PenjualanDTO>(sql, new { Id = id }).ConfigureAwait(false);
            if (master == null) return null;

            var details = await GetDetailsByPenjualanIds(conn, new[] { master.Id }).ConfigureAwait(false);
            master.Details = details.Where(d => d.PenjualanId == master.Id).ToList();
            return master;
        }

        public async Task<List<PenjualanDTO>> GetByCustomerIdAsync(int customerId)
        {
            const string sql = @"
SELECT
    p.Id,
    p.Description,
    p.Tanggal,
    p.CutsID,
    p.StatusID,
    pa.Id AS AddressId,
    pa.AddressLine1,
    pa.AddressLine2,
    pa.City,
    pa.State,
    pa.PostalCode,
    pa.Country,
    pa.NoHP,
    pa.Email,
    c.Name AS CUstomerName
FROM Penjualan p
LEFT JOIN PenjualanAddress pa ON pa.PenjualanId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
WHERE p.CutsID = @CustomerId
ORDER BY p.Tanggal DESC;";

            using var conn = _connectionFactory();
            var masters = (await conn.QueryAsync<PenjualanDTO>(sql, new { CustomerId = customerId }).ConfigureAwait(false)).ToList();
            if (masters.Count == 0) return masters;

            var ids = masters.Select(m => m.Id).ToArray();
            var details = await GetDetailsByPenjualanIds(conn, ids).ConfigureAwait(false);
            var detailsLookup = details.GroupBy(d => d.PenjualanId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var m in masters)
            {
                detailsLookup.TryGetValue(m.Id, out var list);
                m.Details = list ?? new List<PenjualanDetailDTO>();
            }

            return masters;
        }

        public async Task<List<PenjualanDTO>> SearchAsync(string keyword)
        {
            const string sql = @"
SELECT
    p.Id,
    p.Description,
    p.Tanggal,
    p.CutsID,
    p.StatusID,
    pa.Id AS AddressId,
    pa.AddressLine1,
    pa.AddressLine2,
    pa.City,
    pa.State,
    pa.PostalCode,
    pa.Country,
    pa.NoHP,
    pa.Email,
    c.Name AS CUstomerName
FROM Penjualan p
LEFT JOIN PenjualanAddress pa ON pa.PenjualanId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
WHERE p.Description LIKE @q OR c.Name LIKE @q
ORDER BY p.Tanggal DESC;";

            var q = $"%{keyword}%";
            using var conn = _connectionFactory();
            var masters = (await conn.QueryAsync<PenjualanDTO>(sql, new { q }).ConfigureAwait(false)).ToList();
            if (masters.Count == 0) return masters;

            var ids = masters.Select(m => m.Id).ToArray();
            var details = await GetDetailsByPenjualanIds(conn, ids).ConfigureAwait(false);
            var detailsLookup = details.GroupBy(d => d.PenjualanId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var m in masters)
            {
                detailsLookup.TryGetValue(m.Id, out var list);
                m.Details = list ?? new List<PenjualanDetailDTO>();
            }

            return masters;
        }

        private async Task<List<PenjualanDetailDTO>> GetDetailsByPenjualanIds(IDbConnection conn, int[] penjualanIds)
        {
            if (penjualanIds == null || penjualanIds.Length == 0) return new List<PenjualanDetailDTO>();

            const string sql = @"
SELECT Id, PenjualanId, ProductId, ProductName, Quantity, Price, SellingPrice
FROM PenjualanDetail
WHERE PenjualanId IN @Ids
ORDER BY Id;";

            var details = (await conn.QueryAsync<PenjualanDetailDTO>(sql, new { Ids = penjualanIds }).ConfigureAwait(false)).ToList();
            return details;
        }

    }
}
