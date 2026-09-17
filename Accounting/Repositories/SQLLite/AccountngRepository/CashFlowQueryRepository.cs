using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Repositories.SQLLite.AccountngRepository
{
    internal class CashFlowQueryRepository : ICashFlowQueryRepository
    {
        Func<IDbConnection> connectionFactory;
        public CashFlowQueryRepository(Func<IDbConnection> _connectionFactory)
        {
            connectionFactory = _connectionFactory;
        }
        public async Task<List<CashFlowItemDTO>> GetAllAsync()
        {
            try
            {
                var query = "SELECT C.Code, C.Name, C.ParentCode, P.Name as ParentName " +
                    "FROM CashFlowItem C " +
                    "LEFT JOIN CashFlowItem P ON P.Code = C.ParentCode " +
                    "ORDER BY C.Code";

                using (IDbConnection connection = connectionFactory())
                {
                    var lstCashFlowItemDTO = await connection.QueryAsync<CashFlowItemDTO>(query).ConfigureAwait(false);
                    return lstCashFlowItemDTO.ToList();

                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }


        public async Task<CashFlowItemDTO> GetByIdAsync(string Code)
        {
            try
            {
                var query = "SELECT C.Code, C.Name, C.ParentCode, P.Name as ParentName " +
                    "FROM CashFlowItem C " +
                    "LEFT JOIN CashFlowItem P ON P.Code = C.ParentCode " +
                    "WHERE C.Code = @Code " +
                    "ORDER BY C.Code";

                using (IDbConnection connection = connectionFactory())
                {
                    var dto = await connection.QuerySingleOrDefaultAsync<CashFlowItemDTO>(query, new { Code }).ConfigureAwait(false);
                    return dto;
                }
            }
            catch (Exception exp)
            {
                return null;

            }
        }

    }
}
