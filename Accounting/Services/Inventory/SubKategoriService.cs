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
    internal class SubKategoriService: ISubKategoriService
    {
        private readonly ISubKategoriRepository _repo;
        private readonly ISubKategoriQueryRepository _queryRepo;
        private readonly ILogger<SubKategoriService> _logger;

        public SubKategoriService(ISubKategoriRepository repo, ISubKategoriQueryRepository queryRepo, ILogger<SubKategoriService> logger)
        {
            _repo = repo;
            _queryRepo = queryRepo;
            _logger = logger;
        }

        public async Task<List<SubKategoriDTO>> GetAllAsync()
        {
            try
            {
                return await _queryRepo.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll SubKategori failed");
                throw;
            }
        }

        public async Task<SubKategoriDTO?> GetByIdAsync(int id) => await _queryRepo.GetByIdAsync(id).ConfigureAwait(false);

        public async Task<int> CreateAsync(SubKategoriDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Nama)) throw new ArgumentException("Nama wajib diisi", nameof(dto.Nama));
            if (dto.KategoriID <= 0) throw new ArgumentException("Kategori harus dipilih", nameof(dto.KategoriID));

            try
            {
                var entity = new SubKategori
                {
                    Nama = dto.Nama,
                    KategoriID = dto.KategoriID
                };
                return await _repo.CreateAsync(entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create SubKategori failed Nama={Nama}", dto.Nama);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(SubKategoriDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.ID <= 0) throw new ArgumentException("Invalid ID", nameof(dto.ID));
            if (string.IsNullOrWhiteSpace(dto.Nama)) throw new ArgumentException("Nama wajib diisi", nameof(dto.Nama));
            if (dto.KategoriID <= 0) throw new ArgumentException("Kategori harus dipilih", nameof(dto.KategoriID));

            try
            {
                var entity = new SubKategori
                {
                    ID = dto.ID,
                    Nama = dto.Nama,
                    KategoriID = dto.KategoriID
                };
                var rows = await _repo.UpdateAsync(entity).ConfigureAwait(false);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update SubKategori failed ID={ID}", dto.ID);
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
                _logger.LogError(ex, "Delete SubKategori failed ID={ID}", id);
                throw;
            }
        }
    }
}

