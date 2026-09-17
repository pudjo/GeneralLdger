using Accounting.DTO;
using System.Collections.ObjectModel;

namespace Accounting.IRepositories
{
    internal interface IGeneralLedgerQueryRepository
    {
        Task<ObservableCollection<GeneralLedgerDTO>> GetGeneralLedgers(DateTime start,
            DateTime end, string accountCode = "");
        Task<ObservableCollection<GeneralLedgerDTO>> GetLabaRugi(DateTime start,
            DateTime end, string accountCode = "");
        Task<ObservableCollection<LaporanDTO>> GetLaporanAsync(int tahunBerjalan);
    }
}