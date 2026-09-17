
using Microsoft.Extensions.Logging;

namespace Accounting.Services
{
    public class BaseService
    {
        // Properti ini dilindungi (protected) agar bisa diakses langsung oleh kelas anak
        private string sError;        // Constructor ini akan dipanggil oleh Dependency Injection
        protected BaseService()
        {
            Error = string.Empty;
        }

        public string Error
        {
            set { sError = value; }
            get { return sError; }
        }
        

    }
}
