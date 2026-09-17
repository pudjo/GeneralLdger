using Accounting.IRepositories;
using ControlzEx.Standard;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Accounting.Repositories.SQLLite
{
    internal class ConnectionFactory
    {
        
        protected string m_sError;
        
        public ConnectionFactory()
        {
            //uow = _uow;
            m_sError = string.Empty;

        }
        public string Error
        {
            get { return m_sError; }
            set { m_sError = value; }
        }

        public IDbConnection CreateConnection()
        {
            return GetConnection();

        }
    }
}
