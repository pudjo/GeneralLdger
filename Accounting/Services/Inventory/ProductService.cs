using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal  class ProductService : IProductService
    {
        public event EventHandler? ProductsChanged;

        
        private readonly IProductRepository _repo;
        private readonly IProductQueryRepository _queryRepo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, IProductQueryRepository queryRepo, ILogger<ProductService> logger)
        {
            _repo = repo;
            _queryRepo = queryRepo;
            _logger = logger;
        }
        private void OnProductChanged() => ProductsChanged?.Invoke(this, EventArgs.Empty);


        public async Task<List<ProductDTO>> GetAllAsync()
        {
            try
            {
                return await _queryRepo.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync failed");
                throw;
            }
        }

        public async Task<ProductDTO?> GetByIdAsync(int id) => await _queryRepo.GetByIdAsync(id).ConfigureAwait(false);
        public async Task<ProductDTO?> GetByCodeAsync(string code) => await _queryRepo.GetByCodeAsync(code).ConfigureAwait(false);
        public async Task<List<ProductDTO>> SearchAsync(string keyword) => await _queryRepo.SearchAsync(keyword).ConfigureAwait(false);

        public async Task<int> CreateAsync(ProductDTO dto, string createdBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Code is required", nameof(dto.Code));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required", nameof(dto.Name));

            try
            {
                var existing = await _queryRepo.GetByCodeAsync(dto.Code).ConfigureAwait(false);
                if (existing != null) throw new InvalidOperationException($"Product with Code '{dto.Code}' already exists.");

                var entity = new Product
                {
                    Jenis = dto.Jenis, // Assuming Jenis is a required field, set it to a default value or modify as needed
                    Code = dto.Code,
                    Name = dto.Name,
                    PurchasePrice = dto.PurchasePrice,
                    CurrentSellingPrice = dto.CurrentSellingPrice,
                    CurrentStock = dto.CurrentStock,
                };         

            

                var newId = await _repo.CreateAsync(entity).ConfigureAwait(false);
                _logger.LogInformation("Created Product {Code} Id={Id}", dto.Code, newId);
                if (newId > 0) OnProductChanged();
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync failed for Code={Code}", dto.Code);
                throw;
            }
        }
        public async Task<List<ProductDTO>> GetByJenisAsync(int jenisId) => await _queryRepo.GetByJenisAsync(jenisId).ConfigureAwait(false);

        public async Task<bool> UpdateAsync(ProductDTO dto, string updatedBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Id <= 0) throw new ArgumentException("Invalid Id", nameof(dto.Id));

            try
            {
                var entity = new Product
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Name = dto.Name,
                    PurchasePrice = dto.PurchasePrice,
                    CurrentSellingPrice = dto.CurrentSellingPrice,
                    CurrentStock = dto.CurrentStock,
                    Jenis = dto.Jenis,
                };

                var rows = await _repo.UpdateAsync(entity).ConfigureAwait(false);
                
                _logger.LogInformation("Updated Product Id={Id}, Affected={Rows}", dto.Id, rows);
                if (rows > 0) OnProductChanged();
                
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
            try
            {
              var b= await _repo.DeleteAsync(id).ConfigureAwait(false);
                if (b) OnProductChanged();
                return b;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed for Id={Id}", id);
                throw;
            }
        }

        public async Task<int> ImportBunchAsync(List<ProductDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0) return 0;
            var entities = dtos.Select(d => new Product
            {
                Code = d.Code,
                Name = d.Name,
                PurchasePrice = d.PurchasePrice,
                CurrentSellingPrice = d.CurrentSellingPrice,
                CurrentStock = d.CurrentStock
            }).ToList();

            return await _repo.ImportBunch(entities).ConfigureAwait(false);
        }

    }
}
