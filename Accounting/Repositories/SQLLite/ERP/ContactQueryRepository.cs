using Accounting.DTO;
using Accounting.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace Accounting.Repositories.SQLLite.ERP
{
    
    internal class ContactQueryRepository : IContactQueryRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public ContactQueryRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ContactDTO>> GetAllAsync()
        {
            const string sql = @"
SELECT 
    Id,
    Code,
    Name,
    Type,
    Phone,
    Email,
    Address,
    TaxId,
    Company,
    IsActive,
    CreatedAt,
    CreatedBy
FROM Contact
ORDER BY Name;";

            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<ContactDTO>(sql).ConfigureAwait(false);
            return rows.ToList();
        }

        public async Task<ContactDTO> GetByIdAsync(int id)
        {
            const string sql = @"
SELECT 
    Id,
    Code,
    Name,
    Type,
    Phone,
    Email,
    Address,
    TaxId,
    Company,
    IsActive,
    CreatedAt,
    CreatedBy
FROM Contact
WHERE Id = @Id;";

            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<ContactDTO>(sql, new { Id = id }).ConfigureAwait(false);
            return row;
        }

        public async Task<ContactDTO> GetByCodeAsync(string code)
        {
            const string sql = @"
SELECT 
    Id,
    Code,
    Name,
    Type,
    Phone,
    Email,
    Address,
    TaxId,
    Company,
    IsActive,
    CreatedAt,
    CreatedBy
FROM Contact
WHERE Code = @Code;";

            using var conn = _connectionFactory();
            var row = await conn.QuerySingleOrDefaultAsync<ContactDTO>(sql, new { Code = code }).ConfigureAwait(false);
            return row;
        }

        public async Task<List<ContactDTO>> SearchAsync(string keyword)
        {
            const string sql = @"
SELECT 
    Id,
    Code,
    Name,
    Type,
    Phone,
    Email,
    Address,
    TaxId,
    Company,
    IsActive,
    CreatedAt,
    CreatedBy
FROM Contact
WHERE (Name LIKE @p OR Code LIKE @p OR Phone LIKE @p OR Email LIKE @p)
ORDER BY Name;";

            var pattern = $"%{keyword}%";
            using var conn = _connectionFactory();
            var rows = await conn.QueryAsync<ContactDTO>(sql, new { p = pattern }).ConfigureAwait(false);
            return rows.ToList();
        }
    }
    
    
}
