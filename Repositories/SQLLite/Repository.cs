using Accounting.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace Accounting.Repositories.SQLLite
{
    internal class Repository<T> : IRepository<T> where T : class
    {
        protected readonly string _connectionString;

        // String koneksi disuntikkan dari konfigurasi appsettings.json
        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected IDbConnection CreateConnection() => new SQLiteConnection(_connectionString);

        public async Task<List<T>> GetAllAsync()
        {
            // Nama tabel diambil otomatis dari nama Class <T>
            string tableName = typeof(T).Name;
            using var conn = CreateConnection();
            return await conn.QueryAsync<T>($"SELECT * FROM {tableName}");
        }

        public async Task<T> GetByIdAsync(string id)
        {
            string tableName = typeof(T).Name;
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {tableName} WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(T entity)
        {
            using var conn = CreateConnection();
            // Catatan: Untuk Insert, Bapak bisa menulis query spesifik menggunakan Dapper 
            // atau library tambahan seperti Dapper.Contrib agar otomatis tinggal: conn.InsertAsync(entity);
            return 1;
        }

        public async Task<int> UpdateAsync(T entity)
        {
            using var conn = CreateConnection();
            return 1;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            string tableName = typeof(T).Name;
            using var conn = CreateConnection();
            int rows = await conn.ExecuteAsync($"DELETE FROM {tableName} WHERE Id = @Id", new { Id = id });
            return rows>0;
        }
    }
}