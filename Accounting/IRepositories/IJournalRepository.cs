using Accounting.Domain.Entities;
using Accounting.DTO;

namespace Accounting.IRepositories
{
    internal interface IJournalRepository :IRepository<Jurnal>
    {
        Task<int> CreateBunc( List<GeneralLedger> listGL);
        Task<bool> DeleteBunc(List<JurnalDTO> listjurnal);


    }
}
