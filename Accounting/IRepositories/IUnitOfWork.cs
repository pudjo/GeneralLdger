using Accounting.Repositories.SQLLite.AccountngRepository;
using System;
using System.Data;
using System.Threading.Tasks;
namespace Accounting.IRepositories
{
    interface IUnitOfWork:IDisposable
    {
        // Mengakses Repository yang terikat dengan Unit of Work ini
   IJournalRepository JournalRepository { get; }
   IWriteSaldoAwalRepository WriteSaldoAwalRepository { get; }
   

        IGeneralLedgerRepository GeneralLedgerRepository { get; }

        // Manajemen Transaksi
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
//        void BeginTransaction();
        

        // Tambahkan dua baris ini agar bisa diakses di App.xaml.cs
        IDbConnection GetConnection();
        IDbTransaction GetTransaction();
    }
}
