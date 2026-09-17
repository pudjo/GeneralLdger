using Accounting.Domain.Entities;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite.Master;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class CashFlowItemRepository : ICashFlowItemRepository
    {
        private readonly ILogger<CashFlowItemRepository> _logger;
        private Func<IDbConnection> connectionFactory;
        public CashFlowItemRepository(Func<IDbConnection> _connectionFactory, ILogger<CashFlowItemRepository> logger)
        {
            connectionFactory = _connectionFactory;
            _logger = logger;
        }
        public async Task<int> CreateAsync(CashFlowItem entity)
        {
            try
            {
                var query = "INSERT INTO CashFlowItem (Code, Name, ParentCode, GroupType) VALUES (@Code, @Name, @ParentCode, @GroupType)";
                var p = new DynamicParameters();

                p.Add("@Code", entity.Code);
                p.Add("@Name", entity.Name);
                p.Add("@ParentCode", entity.ParentCode);
                p.Add("@GroupType", entity.GroupType);

                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }
            }
            catch (Exception exp)
            {
                _logger?.LogError(exp, "CreateAsync CashFlowItem failed");
                return 0;
            }
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<int> UpdateAsync(CashFlowItem entity)
        {
            try
            {

                var query = "UPDATE CashFlowItem SET Name = @Name, ParentCode = @ParentCode, GroupType = @GroupType WHERE Code = @Code";

                var p = new DynamicParameters();

                p.Add("@Code", entity.Code);
                p.Add("@Name", entity.Name);
                p.Add("@ParentCode", entity.ParentCode);
                p.Add("@GroupType", entity.GroupType);

                using (IDbConnection connection = connectionFactory())
                {
                    return await connection.ExecuteAsync(query, p);
                }

            }
            catch (Exception exp)
            {
                _logger?.LogError(exp, "UpdateAsync CashFlowItem failed");
                return 0;
            }
        }
    }
}
