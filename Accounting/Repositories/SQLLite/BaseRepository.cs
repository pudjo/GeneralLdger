
using System.Data;

namespace Accounting.Repositories.SQLLite
{
    public abstract class BaseRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly Func<IDbTransaction> _transactionFactory;
        private string _error;
        protected BaseRepository(Func<IDbConnection> connectionFactory, Func<IDbTransaction> transactionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));

        }
        // Properti rahasia dapur yang akan digunakan oleh Repository Anak untuk mengeksekusi Query
        protected IDbConnection Connection => _connectionFactory();


        protected IDbTransaction Transaction => _transactionFactory();
        public string Error
        {
            get => _error;
            protected set => _error = value;
        }
    }
}
