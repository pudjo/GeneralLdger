
using Accounting.DTO;


namespace Accounting.IRepositories
{
    internal interface ISaldoAwalReadRepository
    {
        Task<List<SaldoAwalDTO>> GetOnYear(int tahun);
            
        Task<SaldoAwalDTO> GetByID(int id);
    }
}
