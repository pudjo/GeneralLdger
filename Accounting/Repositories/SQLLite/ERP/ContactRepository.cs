using Accounting.Domain.Entities;
using Accounting.IRepositories;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Accounting.Repositories.SQLLite.ERP
{
    internal class ContactRepository:IContactRepository 
    {
        private readonly ILogger<ContactRepository> _logger;
        private readonly Func<IDbConnection> _connectionFactory;

        public ContactRepository(Func<IDbConnection> connectionFactory, ILogger<ContactRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> CreateAsync(Contact entity)
        {
            try
            {
                const string sql = @"
INSERT INTO Contact
(Code, Name, Type, Phone, Email, Address, TaxId, Company, IsActive, CreatedAt, CreatedBy)
VALUES
(@Code, @Name, @Type, @Phone, @Email, @Address, @TaxId, @Company, @IsActive, @CreatedAt, @CreatedBy);
SELECT last_insert_rowid();";

                using var conn = _connectionFactory();
                // Return new id
                var newId = await conn.ExecuteScalarAsync<long>(sql, entity).ConfigureAwait(false);
                return (int)newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync failed for Contact {Code}", entity?.Code);
                return 0;
            }
        }

        public async Task<int> UpdateAsync(Contact entity)
        {
            try
            {
                const string sql = @"
UPDATE Contact SET
    Code = @Code,
    Name = @Name,
    Type = @Type,
    Phone = @Phone,
    Email = @Email,
    Address = @Address,
    TaxId = @TaxId,
    Company = @Company,
    IsActive = @IsActive,
    CreatedBy = @CreatedBy
WHERE Id = @Id;";

                using var conn = _connectionFactory();
                return await conn.ExecuteAsync(sql, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync failed for Contact Id={Id}", entity?.Id);
                return 0;
            }
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                const string sql = "DELETE FROM Contact WHERE Id = @Id;";
                using var conn = _connectionFactory();
                var affected = await conn.ExecuteAsync(sql, new { Id = id }).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed for Contact Id={Id}", id);
                return false;
            }
        }

        // Optional overload if other code expects DeleteAsync(string)
        public async Task<bool> DeleteAsync(string id)
        {
            if (!int.TryParse(id, out var parsed)) return false;
            return await DeleteAsync(parsed).ConfigureAwait(false);
        }

        public async Task<int> ImportBunch(List<Contact> contacts)
        {
            if (contacts == null || contacts.Count == 0) return 0;
            try
            {
                using var conn = _connectionFactory();
                using var tran = conn.BeginTransaction();
                try
                {
                    const string sql = @"
INSERT INTO Contact
(Code, Name, Type, Phone, Email, Address, TaxId, ContactPerson, IsActive, CreatedAt, CreatedBy)
VALUES
(@Code, @Name, @Type, @Phone, @Email, @Address, @TaxId, @ContactPerson, @IsActive, @CreatedAt, @CreatedBy);";

                    foreach (var c in contacts)
                    {
                        await conn.ExecuteAsync(sql, c, tran).ConfigureAwait(false);
                    }

                    tran.Commit();
                    return contacts.Count;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logger.LogError(ex, "ImportBunch failed for contacts");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportBunch connection error");
                return 0;
            }
        }
    }
}
