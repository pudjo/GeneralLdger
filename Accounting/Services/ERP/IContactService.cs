using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.ERP
{
    internal interface IContactService
    {
        Task<List<ContactDTO>> GetAllAsync();
        Task<ContactDTO?> GetByIdAsync(int id);
        Task<ContactDTO?> GetByCodeAsync(string code);
        Task<List<ContactDTO>> SearchAsync(string keyword);

        Task<int> CreateAsync(ContactDTO dto, string createdBy);
        Task<bool> UpdateAsync(ContactDTO dto, string updatedBy);
        Task<bool> DeleteAsync(int id);
        // Notification event - raised when customers are added/updated/deleted/imported
        
        
    }
}
