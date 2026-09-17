using Accounting.Domain.Entities;
using Accounting.IRepositories.Koperasi;
using Accounting.Repositories.SQLLite.Master;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Accounting.Repositories.SQLLite.Koperasi
{
    internal class AnggotaRepository :  IAnggotaRepository
    {
        private readonly ILogger<AnggotaRepository> _logger;
        Func<IDbConnection> connectionFactory;
        public AnggotaRepository(Func<IDbConnection> _connectionFactory,  ILogger<AnggotaRepository> logger)
        {
            this.connectionFactory = _connectionFactory;
            _logger = logger;
        }


        public async Task<int> CreateAsync(Anggota entity)
        {
            try
            {
                var query = "INSERT  into Anggota (Nama,NIK,Alamat,NoTelepon,JenisKelamin) values (@Nama,@NIK,@Alamat,@NoTelepon,@JenisKelamin)";
                var p = new DynamicParameters();


                
                p.Add("@Nama", entity.Nama); 
                p.Add("@NIK", entity.NIK);
                p.Add("@Alamat", entity.Alamat);
                p.Add("@NoTelepon", entity.NoTelepon);
                p.Add("@JenisKelamin", entity.JenisKelamin);

                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
              //  Error = exp.Message;
                return 0;

            }
        }
        public async Task<int> UpdateAsync(Anggota entity)
        {
            try
            {
                var query = "UPDATE Anggota SET Nama=@Nama,NIK=@NIK,Alamat=@Alamat,NoTelepon=@NoTelepon,JenisKelamin=@JenisKelamin wherre id=@id";

                var p = new DynamicParameters();



                p.Add("@Nama", entity.Nama);
                p.Add("@NIK", entity.NIK);
                p.Add("@Alamat", entity.Alamat);
                p.Add("@NoTelepon", entity.NoTelepon);
                p.Add("@JenisKelamin", entity.JenisKelamin);
                p.Add("@Id", entity.Id);
                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                
                return 0;

            }

        }
        public Task<bool> DeleteAsync(string id)
        {
            try
            {
                var query = "DELETE Anggota  where id=@id";

                var p = new DynamicParameters();
                p.Add("@Id", id);
                return Task.FromResult(true);
            }
            catch (Exception exp)
            {
                
                return Task.FromResult(false);
             }
        }
        public async Task<Anggota> GetByIdAsync(string id)
        {
            try
            {

                var query = "SELECT * from Anggota where Id= Id = @Id";
                var p = new DynamicParameters();
                using (IDbConnection connection = connectionFactory())
                {
                    var Anggota = connection.QuerySingle<Anggota>(query, new { Id = id });
                    return Anggota;
                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
