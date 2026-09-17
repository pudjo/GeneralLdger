using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    internal interface IContactQueryRepository
    {
        Task<List<ContactDTO>> GetAllAsync();
        Task<ContactDTO> GetByIdAsync(int id);
        Task<ContactDTO> GetByCodeAsync(string code);
        Task<List<ContactDTO>> SearchAsync(string keyword);
    }
}
