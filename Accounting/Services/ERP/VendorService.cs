using Accounting.Domain.Enum;
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
    internal class VendorService:    ContactService, IVendorService
    {
        public event EventHandler? VendorsChanged;

        public VendorService(
            IContactRepository contactRepository,
            IContactQueryRepository contactQueryRepository,
            ILogger<ContactService> logger)
            : base(contactRepository, contactQueryRepository, logger)

        {

        }
        private void OnVendorsChanged() => VendorsChanged?.Invoke(this, EventArgs.Empty);

        // Query: get all Vendors only
        public async Task<List<ContactDTO>> GetAllVendorsAsync()
        {
            var all = await base.GetAllAsync().ConfigureAwait(false);
            return all?.Where(c => c.Type == ContactType.Vendor).ToList() ?? new List<ContactDTO>();
        }

        // Query by id but ensure returned item is a Vendor
        public async Task<ContactDTO?> GetVendorByIdAsync(int id)
        {
            var dto = await base.GetByIdAsync(id).ConfigureAwait(false);
            return dto != null && dto.Type == ContactType.Vendor ? dto : null;
        }

        // Query by code but ensure returned item is a Vendor
        public async Task<ContactDTO?> GetVendorByCodeAsync(string code)
        {
            var dto = await base.GetByCodeAsync(code).ConfigureAwait(false);
            return dto != null && dto.Type == ContactType.Vendor ? dto : null;
        }

        // Search but restrict to Vendors
        public async Task<List<ContactDTO>> SearchVendorsAsync(string keyword)
        {
            var list = await base.SearchAsync(keyword).ConfigureAwait(false);
            return list?.Where(c => c.Type == ContactType.Vendor).ToList() ?? new List<ContactDTO>();
        }

        // Create Vendor: force ContactType.Vendor
        public async Task<int> CreateVendorAsync(ContactDTO dto, string createdBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            dto.Type = ContactType.Vendor;
            var newId = await base.CreateAsync(dto, createdBy).ConfigureAwait(false);
            if (newId > 0) OnVendorsChanged();
            return newId;


        }

        // Update Vendor: ensure type remains Vendor
        public async Task<bool> UpdateVendorAsync(ContactDTO dto, string updatedBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            dto.Type = ContactType.Vendor;
            var ok = await base.UpdateAsync(dto, updatedBy).ConfigureAwait(false);
            if (ok) OnVendorsChanged();
            return ok;

        }

        // Delete Vendor (delegates to base)
        public async Task<bool> DeleteVendorAsync(int id)
        {
            var ok = await base.DeleteAsync(id).ConfigureAwait(false);
            if (ok) OnVendorsChanged();
            return ok;

        }

        // Import bunch of Vendors from DTOs (business rule: mark type = Vendor)
        public async Task<int> ImportVendorsAsync(List<ContactDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0) return 0;
            foreach (var d in dtos) d.Type = ContactType.Vendor;
            int imported = 0;
            foreach (var dto in dtos)
            {
                try
                {
                    await CreateVendorAsync(dto, dto.CreatedBy ?? "import").ConfigureAwait(false);
                    imported++;
                }
                catch
                {
                    // skip duplicates/errors
                }
            }
            if (imported > 0) OnVendorsChanged();
            return imported;
        }

    }
}
