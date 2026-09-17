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
    internal class JenisService : IJenisService
    {
        private readonly IJenisRepository _repo;
        private readonly IJenisQueryRepository _queryRepo;
        private readonly ILogger<JenisService> _logger;

        public JenisService(IJenisRepository repo, IJenisQueryRepository queryRepo, ILogger<JenisService> logger)
        {
            _repo = repo;
            _queryRepo = queryRepo;
            _logger = logger;
        }

        public async Task<List<JenisDTO>> GetAllAsync()
        {
            try
            {
                return await _queryRepo.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll Jenis failed");
                throw;
            }
        }

        public async Task<JenisDTO?> GetByIdAsync(int id) => await _queryRepo.GetByIdAsync(id).ConfigureAwait(false);

        public async Task<int> CreateAsync(JenisDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nama)) throw new ArgumentException("Nama wajib diisi", nameof(dto.Nama));

            try
            {
                var entity = new Jenis { Nama = dto.Nama,Kode=dto.Kode, ParentID = dto.ParentID };
                return await _repo.CreateAsync(entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Jenis failed Nama={Nama}", dto.Nama);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(JenisDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.ID <= 0) throw new ArgumentException("Invalid ID", nameof(dto.ID));
            if (string.IsNullOrWhiteSpace(dto.Nama)) throw new ArgumentException("Nama wajib diisi", nameof(dto.Nama));

            try
            {
                var entity = new Jenis { ID = dto.ID, Nama = dto.Nama,Kode=dto.Kode, ParentID = dto.ParentID };
                var rows = await _repo.UpdateAsync(entity).ConfigureAwait(false);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Jenis failed ID={ID}", dto.ID);
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
                _logger.LogError(ex, "Delete Jenis failed ID={ID}", id);
                throw;
            }
        }

    }
}
