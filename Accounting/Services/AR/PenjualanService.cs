using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accounting.DTO.AR;
using Accounting.IRepositories.AR;
using Microsoft.Extensions.Logging;

namespace Accounting.Services.AR
{
    internal class PenjualanService : IPenjualanService
    {
        private readonly IPenjualanRepository _repo;
        private readonly IPenjualanQueryRepository _queryRepo;
        private readonly ILogger<PenjualanService> _logger;

        public PenjualanService(
            IPenjualanRepository repo,
            IPenjualanQueryRepository queryRepo,
            ILogger<PenjualanService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _queryRepo = queryRepo ?? throw new ArgumentNullException(nameof(queryRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details)
        {
            if (penjualan == null) throw new ArgumentNullException(nameof(penjualan));

            try
            {
                var newId = await _repo.CreateAsync(penjualan, details).ConfigureAwait(false);
                _logger.LogInformation("Created Penjualan Id={Id} Description={Description}", newId, penjualan?.Description);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync failed for Penjualan Description={Description}", penjualan?.Description);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(PenjualanDTO penjualan, IEnumerable<PenjualanDetailDTO> details)
        {
            if (penjualan == null) throw new ArgumentNullException(nameof(penjualan));
            if (penjualan.Id <= 0) throw new ArgumentException("Penjualan.Id must be provided for update", nameof(penjualan));

            try
            {
                var updatedId = await _repo.UpdateAsync(penjualan, details).ConfigureAwait(false);
                var success = updatedId > 0;
                _logger.LogInformation("Updated Penjualan Id={Id} Success={Success}", penjualan.Id, success);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync failed for Penjualan Id={Id}", penjualan.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("id must be greater than zero", nameof(id));

            try
            {
                var result = await _repo.DeleteAsync(id).ConfigureAwait(false);
                _logger.LogInformation("Deleted Penjualan Id={Id} Result={Result}", id, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed for Penjualan Id={Id}", id);
                throw;
            }
        }

        // Query/read operations that use the query repository

        public async Task<List<PenjualanDTO>> GetAllAsync()
        {
            try
            {
                return await _queryRepo.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync failed in PenjualanService");
                throw;
            }
        }

        public async Task<PenjualanDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _queryRepo.GetByIdAsync(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed for Id={Id}", id);
                throw;
            }
        }

        public async Task<List<PenjualanDTO>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _queryRepo.GetByCustomerIdAsync(customerId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByCustomerIdAsync failed for CustomerId={CustomerId}", customerId);
                throw;
            }
        }

        public async Task<List<PenjualanDTO>> SearchAsync(string keyword)
        {
            try
            {
                return await _queryRepo.SearchAsync(keyword).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchAsync failed for Keyword={Keyword}", keyword);
                throw;
            }
        }
    }
}