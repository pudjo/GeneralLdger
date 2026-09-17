using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.ERP
{
    public  interface IVendorService
    {
        Task<List<ContactDTO>> GetAllVendorsAsync();
        Task<ContactDTO?> GetVendorByIdAsync(int id);


        // Query by code but ensure returned item is a Vendor
        Task<ContactDTO?> GetVendorByCodeAsync(string code);
        Task<List<ContactDTO>> SearchVendorsAsync(string keyword);
        Task<int> CreateVendorAsync(ContactDTO dto, string createdBy);
        Task<bool> UpdateVendorAsync(ContactDTO dto, string updatedBy);
        Task<bool> DeleteVendorAsync(int id);
        Task<int> ImportVendorsAsync(List<ContactDTO> dtos);
        event EventHandler? VendorsChanged;
    }
}
