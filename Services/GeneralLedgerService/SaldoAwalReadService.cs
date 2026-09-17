using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite.AccountngRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.GeneralLedgerService
{
    
    internal class SaldoAwalReadService
    {
        ISaldoAwalReadRepository saldoAwalReadRepository;
        public SaldoAwalReadService(ISaldoAwalReadRepository _saldoAwalReadRepository)
        {
            this.saldoAwalReadRepository = _saldoAwalReadRepository;
        }
        public async Task<List<SaldoAwalDTO>> GetOnYear(int year)
        {
            return await saldoAwalReadRepository.GetOnYear(year);

        }

    }
}
