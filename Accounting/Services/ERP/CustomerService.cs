using Accounting.Domain.Enum;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Services.Accounts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.ERP
{
    
    internal class CustomerService :   ContactService, ICustomerService
    {
        public event EventHandler? CustomersChanged;
        private readonly ILogger<ContactService> _logger;
        public CustomerService(
            IContactRepository contactRepository,
            IContactQueryRepository contactQueryRepository,
            ILogger<ContactService> logger)
            : base(contactRepository, contactQueryRepository, logger)
              

        {
            _logger = logger;   
        }
        private void OnCustomersChanged() => CustomersChanged?.Invoke(this, EventArgs.Empty);

        // Query: get all customers only
        public async Task<List<ContactDTO>> GetAllCustomersAsync()
        {
            var all = await base.GetAllAsync().ConfigureAwait(false);
            return all?.Where(c => c.Type == ContactType.Customer).ToList() ?? new List<ContactDTO>();
        }

        // Query by id but ensure returned item is a customer
        public async Task<ContactDTO?> GetCustomerByIdAsync(int id)
        {
            var dto = await base.GetByIdAsync(id).ConfigureAwait(false);
            return dto != null && dto.Type == ContactType.Customer ? dto : null;
        }

        // Query by code but ensure returned item is a customer
        public async Task<ContactDTO?> GetCustomerByCodeAsync(string code)
        {
            var dto = await base.GetByCodeAsync(code).ConfigureAwait(false);
            return dto != null && dto.Type == ContactType.Customer ? dto : null;
        }

        // Search but restrict to customers
        public async Task<List<ContactDTO>> SearchCustomersAsync(string keyword)
        {
            var list = await base.SearchAsync(keyword).ConfigureAwait(false);
            return list?.Where(c => c.Type == ContactType.Customer).ToList() ?? new List<ContactDTO>();
        }

        // Create customer: force ContactType.Customer
        public async Task<int> CreateCustomerAsync(ContactDTO dto, string createdBy)
        {
            try
            {
                if (dto == null) throw new ArgumentNullException(nameof(dto));
                dto.Type = ContactType.Customer;
                var newId = await base.CreateAsync(dto, createdBy).ConfigureAwait(false);
                if (newId > 0) OnCustomersChanged();
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer with code {Code}", dto?.Code);
                return 0;


            }
        }

        // Update customer: ensure type remains Customer
        public async Task<bool> UpdateCustomerAsync(ContactDTO dto, string updatedBy)
        {
            try
            {
                if (dto == null) throw new ArgumentNullException(nameof(dto));
                dto.Type = ContactType.Customer;
                var ok = await base.UpdateAsync(dto, updatedBy).ConfigureAwait(false);
                if (ok) OnCustomersChanged();
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with id {Id}", dto?.Id);
                return false;

            }
        }

        // Delete customer (delegates to base)
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {

                var ok = await base.DeleteAsync(id).ConfigureAwait(false);
                if (ok) OnCustomersChanged();
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with id {Id}", id);
                return false;

            }
        }

        // Import bunch of customers from DTOs (business rule: mark type = Customer)
        public async Task<int> ImportCustomersAsync(List<ContactDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0) return 0;
            foreach (var d in dtos) d.Type = ContactType.Customer;
            int imported = 0;
            foreach (var dto in dtos)
            {
                try
                {
                    await CreateCustomerAsync(dto, dto.CreatedBy ?? "import").ConfigureAwait(false);
                    imported++;
                }
                catch
                {
                    // skip duplicates/errors
                }
            }
            if (imported > 0) OnCustomersChanged();
            return imported;
        }

    }
}

