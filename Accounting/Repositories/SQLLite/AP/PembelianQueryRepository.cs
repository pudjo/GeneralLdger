using Accounting.Domain.Entities.AP;
using Accounting.DTO.AP;
using Accounting.DTO.AR;
using Accounting.IRepositories.AP;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.AP
{
    internal class PembelianQueryRepository:IPembelianQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public PembelianQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<List<PembelianDTO>> GetAllAsync()
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
FROM Pembelian p
LEFT JOIN PembelianAddress pa ON pa.PembelianId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
ORDER BY p.Tanggal DESC;";

            using var conn = _connectionFactory();
            var masters = (await conn.QueryAsync<PembelianDTO>(sql).ConfigureAwait(false)).ToList();

            if (masters.Count == 0) return masters;

            var ids = masters.Select(m => m.Id).ToArray();
            var details = await GetDetailsByPembelianIds(conn, ids).ConfigureAwait(false);

            // assign details to masters
            var detailsLookup = details.GroupBy(d => d.PembelianId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var m in masters)
            {
                detailsLookup.TryGetValue(m.Id, out var list);
                m.Details = list ?? new List<PembelianDetailDTO>();
            }

            return masters;
        }

        public async Task<PembelianDTO?> GetByIdAsync(int id)
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
FROM Pembelian p
LEFT JOIN PembelianAddress pa ON pa.PembelianId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
WHERE p.Id = @Id;";

            using var conn = _connectionFactory();
            var master = await conn.QuerySingleOrDefaultAsync<PembelianDTO>(sql, new { Id = id }).ConfigureAwait(false);
            if (master == null) return null;

            var details = await GetDetailsByPembelianIds(conn, new[] { master.Id }).ConfigureAwait(false);
            master.Details = details.Where(d => d.PembelianId == master.Id).ToList();
            return master;
        }

        public async Task<List<PembelianDTO>> GetByCustomerIdAsync(int customerId)
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
FROM Pembelian p
LEFT JOIN PembelianAddress pa ON pa.PembelianId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
WHERE p.CutsID = @CustomerId
ORDER BY p.Tanggal DESC;";

            using var conn = _connectionFactory();
            var masters = (await conn.QueryAsync<PembelianDTO>(sql, new { CustomerId = customerId }).ConfigureAwait(false)).ToList();
            if (masters.Count == 0) return masters;

            var ids = masters.Select(m => m.Id).ToArray();
            var details = await GetDetailsByPembelianIds(conn, ids).ConfigureAwait(false);
            var detailsLookup = details.GroupBy(d => d.PembelianId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var m in masters)
            {
                detailsLookup.TryGetValue(m.Id, out var list);
                m.Details = list ?? new List<PembelianDetailDTO>();
            }

            return masters;
        }

        public async Task<List<PembelianDTO>> SearchAsync(string keyword)
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
FROM Pembelian p
LEFT JOIN PembelianAddress pa ON pa.PembelianId = p.Id
LEFT JOIN Contact c ON c.Id = p.CutsID
WHERE p.Description LIKE @q OR c.Name LIKE @q
ORDER BY p.Tanggal DESC;";

            var q = $"%{keyword}%";
            using var conn = _connectionFactory();
            var masters = (await conn.QueryAsync<PembelianDTO>(sql, new { q }).ConfigureAwait(false)).ToList();
            if (masters.Count == 0) return masters;

            var ids = masters.Select(m => m.Id).ToArray();
            var details = await GetDetailsByPembelianIds(conn, ids).ConfigureAwait(false);
            var detailsLookup = details.GroupBy(d => d.PembelianId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var m in masters)
            {
                detailsLookup.TryGetValue(m.Id, out var list);
                m.Details = list ?? new List<PembelianDetailDTO>();
            }

            return masters;
        }

        private async Task<List<PembelianDetailDTO>> GetDetailsByPembelianIds(IDbConnection conn, int[] PembelianIds)
        {
            if (PembelianIds == null || PembelianIds.Length == 0) return new List<PembelianDetailDTO>();

            const string sql = @"
SELECT Id, PembelianId, ProductId, ProductName, Quantity, Price, SellingPrice
FROM PembelianDetail
WHERE PembelianId IN @Ids
ORDER BY Id;";

            var details = (await conn.QueryAsync<PembelianDetailDTO>(sql, new { Ids = PembelianIds }).ConfigureAwait(false)).ToList();
            return details;
        }

        public Task<List<PembelianDTO>> GetByVendorIdAsync(int VendorId)
        {
            throw new NotImplementedException();
        }
    }
}
