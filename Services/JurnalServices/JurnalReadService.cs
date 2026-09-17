using Accounting.DTO;
using Accounting.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.JurnalServices
{
    internal class JurnalReadService
    {
        private readonly IJournalReadRepository jurnalReadRepository;
        public JurnalReadService(IJournalReadRepository _jurnalReadRepository)
        {
            this.jurnalReadRepository = _jurnalReadRepository;
        }

        public async Task<List<JurnalDTO>> GetJurnalAndDetail(DateTime start, DateTime end,int id = 0)
        {
            return await jurnalReadRepository.GetJurnalAndDetail(start, end ,id);
        }

    }
}
