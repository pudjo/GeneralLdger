using Accounting.Domain.Entities;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite.AccountngRepository;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.Master
{
    internal class JabatanWriteRepository : BaseRepository, IJabatanWriteRepository
    {
        private readonly ILogger<JabatanWriteRepository> _logger;
        public JabatanWriteRepository(ILogger<JabatanWriteRepository> logger,
                Func<IDbConnection> connectionFactory)
        : base(connectionFactory, () => null)
        {
            _logger = logger;
        }

        public async Task<int> CreateAsync(Jabatan entity)
        {
            try
            {
                var query = "INSERT  into Jabatan (Name) values (@Name)";
                var p = new DynamicParameters();

                p.Add("@Name", entity.Name);

                using (var connection = Connection)
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return 0;

            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var query = "DLEETE Jabatan where ID=@ID ";
                var p = new DynamicParameters();

                p.Add("@ID", id);

                using (var connection = Connection)
                {
                    int ret = await connection.ExecuteAsync(query, p);
                    return ret > 0;
                }
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return false;

            }
        }


        public async Task<int> UpdateAsync(Jabatan entity)
        {
            try
            {
                var query = "UPDATE Jabatan SET Name =@Name where ID= @id  ";
                var p = new DynamicParameters();

                p.Add("@Name", entity.Name);
                p.Add("@id", entity.Id);


                using (var connection = Connection)
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                Error = exp.Message;
                return 0;

            }
        }
    }
}
