using Accounting.Domain.Entities;
using Accounting.IRepositories.Koperasi;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Koperasi
{


    internal class AnggotaQueryRepository:  IAnggotaQueryRepository
    {
        private readonly ILogger<AnggotaQueryRepository> _logger;
        Func<IDbConnection> connectionFactory;

        public AnggotaQueryRepository(Func<IDbConnection> _connectionFactory,
                ILogger<AnggotaQueryRepository> logger)
    {
            connectionFactory=_connectionFactory;

        _logger = logger;
    }

    
        public async Task<List<Anggota>> GetAllAsync()
        {
            try
            {

                var query = "SELECT * from Anggota order by Anggota.Name";
                using (IDbConnection connection = connectionFactory())
                {
                    var lstAnggota = await connection.QueryAsync<Anggota>(query).ConfigureAwait(false);
                    return lstAnggota.ToList();

                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }
        public async Task<List<Anggota>> GetAllAsyncTest()
        {
            try
            {


                List<Anggota> sampleAnggotas = new List<Anggota>();

 
                return sampleAnggotas;
            }
            catch (Exception exp)
            {
                return null;

            }
        }

        public Task<Anggota> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        Task<List<AnggotaDTO>> IAnggotaQueryRepository.GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
