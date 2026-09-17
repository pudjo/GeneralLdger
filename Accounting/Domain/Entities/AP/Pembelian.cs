using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities.AP
{
    internal class Pembelian
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime Tanggal { get; set; }
        public int VendorID
        {
            get; set;
        }
        public int StatusID
        {
            get; set;
        }

    }
}
