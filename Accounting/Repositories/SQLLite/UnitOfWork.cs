using Accounting.IRepositories;
using Accounting.Repositories.SQLLite.AccountngRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace Accounting.Repositories.SQLLite
{
    internal class UnitOfWork:IUnitOfWork
    {
        private readonly string _connectionString;
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;

        // Properti internal untuk caching instance repository
        private IJournalRepository _ijournalRepository;
        private IGeneralLedgerRepository _igeneralLedgerRepository;
        private IWriteSaldoAwalRepository _iwritesaldoAwalRepository;
        private ISaldoAwalReadRepository _isaldoAwalReadRepository;
        public UnitOfWork(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Lazy loading untuk repository, memastikan instance dibuat menggunakan koneksi & transaksi yang sama
        public IJournalRepository JournalRepository =>
            _ijournalRepository ??= new JurnalRepository(() =>
                _connection, () => _transaction);

        public IGeneralLedgerRepository GeneralLedgerRepository =>
            _igeneralLedgerRepository ??= new GeneralLedgerRepository(() => _connection, () => _transaction);

        public IWriteSaldoAwalRepository WriteSaldoAwalRepository =>
           _iwritesaldoAwalRepository ??= new WriteSaldoAwalRepository(() => _connection, () => _transaction);

        public async Task BeginTransactionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
            }

            if (_connection.State == ConnectionState.Closed)
            {
                await Task.Run(() => _connection.Open());

                // Pastikan mode WAL aktif agar tidak mudah mengunci
            /*    using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA journal_mode = WAL;";
                    command.ExecuteNonQuery();
                }(*/
            }

            // 💡 SOLUSI KRISITIAL: Jika transaksi sudah ada, keluar dari fungsi (jangan tumpuk transaksi)
            if (_transaction != null)
            {
                return;
            }
        
            _transaction = _connection.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaksi belum dimulai.");

            await Task.Run(() => _transaction.Commit());
            DisposeTransaction();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await Task.Run(() => _transaction.Rollback());
                DisposeTransaction();
            }
        }

        private void DisposeTransaction()
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeTransaction();
                    _connection?.Dispose();
                    _connection = null;
                }
                _disposed = true;
            }
        }


        

        public  IDbConnection GetConnection()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection(_connectionString);
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();

                }

                return _connection;
            } catch(Exception exp)
            {
                return null;
            }
        }

        public IDbTransaction GetTransaction()
        {
            return _transaction;  //return _connection.BeginTransaction();
        }
    }
}
