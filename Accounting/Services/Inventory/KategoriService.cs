using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Inventory
{
    internal class KategoriService : IKategoriService
    {
        private readonly IKategoriRepository _repo;
        private readonly IKategoriQueryRepository _queryRepo;
        private readonly ILogger<KategoriService> _logger;

        public KategoriService(IKategoriRepository repo, IKategoriQueryRepository queryRepo, ILogger<KategoriService> logger)
        {
            _repo = repo;
            _queryRepo = queryRepo;
            _logger = logger;
        }

        public async Task<List<KategoriDTO>> GetAllAsync()
        {
            try
            {
                return await _queryRepo.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll Kategori failed");
                throw;
            }
        }

        public async Task<KategoriDTO?> GetByIdAsync(int id) => await _queryRepo.GetByIdAsync(id).ConfigureAwait(false);

        public async Task<int> CreateAsync(KategoriDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nama)) throw new ArgumentException("Nama wajib diisi", nameof(dto.Nama));

            try
            {
                var existing = await _queryRepo.GetByNameAsync(dto.Nama).ConfigureAwait(false);
                if (existing != null) throw new InvalidOperationException("Kategori sudah ada.");

                var entity = new Kategori { Nama = dto.Nama };
                return await _repo.CreateAsync(entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Kategori failed Nama={Nama}", dto.Nama);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(KategoriDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.ID <= 0) throw new ArgumentException("Invalid ID", nameof(dto.ID));

            try
            {
                var entity = new Kategori { ID = dto.ID, Nama = dto.Nama };
                var rows = await _repo.UpdateAsync(entity).ConfigureAwait(false);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Kategori failed ID={ID}", dto.ID);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _repo.DeleteAsync(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Kategori failed ID={ID}", id);
                throw;
            }
        }
    }

}

