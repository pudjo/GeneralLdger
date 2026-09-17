using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.ERP
{
    abstract class ContactService:IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactQueryRepository _contactQueryRepository;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IContactRepository contactRepository,
            IContactQueryRepository contactQueryRepository,
            ILogger<ContactService> logger)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _contactQueryRepository = contactQueryRepository ?? throw new ArgumentNullException(nameof(contactQueryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Query methods (read-only)
        public async Task<List<ContactDTO>> GetAllAsync()
        {
            try
            {
                return await _contactQueryRepository.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync failed");
                throw;
            }
        }

        public async Task<ContactDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _contactQueryRepository.GetByIdAsync(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed for Id={Id}", id);
                throw;
            }
        }

        public async Task<ContactDTO?> GetByCodeAsync(string code)
        {
            try
            {
                return await _contactQueryRepository.GetByCodeAsync(code).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByCodeAsync failed for Code={Code}", code);
                throw;
            }
        }

        public async Task<List<ContactDTO>> SearchAsync(string keyword)
        {
            try
            {
                return await _contactQueryRepository.SearchAsync(keyword).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchAsync failed for keyword={Keyword}", keyword);
                throw;
            }
        }

        // Business operations (create/update/delete)
        public async Task<int> CreateAsync(ContactDTO dto, string createdBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Code is required", nameof(dto.Code));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required", nameof(dto.Name));

            try
            {
                // Business rule: code must be unique
                var existing = await _contactQueryRepository.GetByCodeAsync(dto.Code).ConfigureAwait(false);
                if (existing != null)
                {
                    throw new InvalidOperationException($"Contact with Code '{dto.Code}' already exists.");
                }

                var entity = new Contact
                {
                    Code = dto.Code,
                    Name = dto.Name,
                    Type = dto.Type,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    TaxId = dto.TaxId,
                    Company = dto.Company,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy ?? string.Empty
                };

                var newId = await _contactRepository.CreateAsync(entity).ConfigureAwait(false);
                _logger.LogInformation("Created Contact {Code} Id={Id}", dto.Code, newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync failed for Code={Code}", dto.Code);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(ContactDTO dto, string updatedBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Id <= 0) throw new ArgumentException("Invalid Id", nameof(dto.Id));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required", nameof(dto.Name));

            try
            {
                // Optionally ensure code uniqueness when code changed
                var byCode = await _contactQueryRepository.GetByCodeAsync(dto.Code).ConfigureAwait(false);
                if (byCode != null && byCode.Id != dto.Id)
                {
                    throw new InvalidOperationException($"Another contact with Code '{dto.Code}' already exists.");
                }

                var entity = new Contact
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Name = dto.Name,
                    Type = dto.Type,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    TaxId = dto.TaxId,
                    Company = dto.Company,
                    IsActive = dto.IsActive,
                    CreatedAt = dto.CreatedAt,
                    CreatedBy = dto.CreatedBy
                };

                var rows = await _contactRepository.UpdateAsync(entity).ConfigureAwait(false);
                _logger.LogInformation("Updated Contact Id={Id}, Affected={Rows}", dto.Id, rows);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync failed for Id={Id}", dto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id", nameof(id));

            try
            {
                var result = await _contactRepository.DeleteAsync(id).ConfigureAwait(false);
                _logger.LogInformation("Deleted Contact Id={Id} Success={Result}", id, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed for Id={Id}", id);
                throw;
            }
        }

        // Import many contacts (business rule: skip duplicates by Code)
    
    }
}
