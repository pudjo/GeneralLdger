using Accounting.Domain.Entities;
using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.IRepositories
{
    internal interface IJournalReadRepository
    {

        Task<List<JurnalDTO>> GetJurnalAndDetail(DateTime start,
            DateTime end, int id = 0);
        Task<Jurnal> GetByID(int id);
        
    }
}
