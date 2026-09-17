using Accounting.Domain.Entities;
using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories.Koperasi
{
    interface IAnggotaQueryRepository
    {
        Task<List<AnggotaDTO>> GetAllAsync();
        Task<Anggota> GetByIdAsync(string id);

    }
}
