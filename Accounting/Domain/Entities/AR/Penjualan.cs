using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities.AR
{
    internal class Penjualan
    {
        public int Id { get; set; }
      
        public string Description { get; set; }

        public DateTime Tanggal { get; set; }
        public int CutsID
        {
            get; set;
        }
        public int StatusID
        {
            get; set;
        }

    }
}
