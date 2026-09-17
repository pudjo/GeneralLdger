using Accounting.Domain.Enum;
using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.ERP
{
    public  interface ICustomerService
    {
        Task<List<ContactDTO>> GetAllCustomersAsync();
        Task<ContactDTO?> GetCustomerByIdAsync(int id);


        // Query by code but ensure returned item is a customer
        Task<ContactDTO?> GetCustomerByCodeAsync(string code);
        Task<List<ContactDTO>> SearchCustomersAsync(string keyword);
        Task<int> CreateCustomerAsync(ContactDTO dto, string createdBy);
        Task<bool> UpdateCustomerAsync(ContactDTO dto, string updatedBy);
        Task<bool> DeleteCustomerAsync(int id);
        Task<int> ImportCustomersAsync(List<ContactDTO> dtos);
        event EventHandler? CustomersChanged;
    }
}
