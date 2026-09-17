using Accounting.Domain.Entities;
using Accounting.IRepositories;
using Accounting.IRepositories.Inventory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal class PriceLogService : IPriceLogService
    {
        private readonly IPriceLogRepository _priceLogRepo;
        private readonly IProductRepository _productRepo;
        private readonly IProductQueryRepository _productQuery;
        private readonly ILogger<PriceLogService> _logger;

        public PriceLogService(IPriceLogRepository priceLogRepo,
                               IProductRepository productRepo,
                               IProductQueryRepository productQuery,
                               ILogger<PriceLogService> logger)
        {
            _priceLogRepo = priceLogRepo ?? throw new ArgumentNullException(nameof(priceLogRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _productQuery = productQuery ?? throw new ArgumentNullException(nameof(productQuery));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SavePriceAsync(int productId, decimal newHargaJual, int createdBy)
        {
            try
            {
                // 1. Get current product data
                var productDto = await _productQuery.GetByIdAsync(productId).ConfigureAwait(false);
                if (productDto == null)
                {
                    _logger.LogWarning("Product not found for SavePrice: Id={Id}", productId);
                    return false;
                }

                // 2. Insert price log
                var priceLog = new PriceLog
                {
                    ProductId = productId,
                    Tanggal = DateTime.Now,
                    HargaJual = newHargaJual,
                    CreatedBy = createdBy
                };

                var newLogId = await _priceLogRepo.CreateAsync(priceLog).ConfigureAwait(false);
                if (newLogId <= 0)
                {
                    _logger.LogError("Failed to insert PriceLog for ProductId={Id}", productId);
                    return false;
                }

                // 3. Update product current selling price (preserve other fields)
                var productEntity = new Domain.Entities.Product
                {
                    Id = productDto.Id,
                    Code = productDto.Code,
                    Name = productDto.Name,
                    PurchasePrice = productDto.PurchasePrice,
                    CurrentSellingPrice = newHargaJual,
                    CurrentStock = productDto.CurrentStock
                };

                var rows = await _productRepo.UpdateAsync(productEntity).ConfigureAwait(false);
                if (rows <= 0)
                {
                    _logger.LogError("Failed to update Product price for Id={Id}", productId);
                    return false;
                }

                _logger.LogInformation("Saved PriceLog Id={LogId} and updated Product Id={Id} price to {Price}", newLogId, productId, newHargaJual);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SavePriceAsync for ProductId={Id}", productId);
                return false;
            }
        }

    }
}
