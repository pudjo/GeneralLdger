using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities
{
    internal class SubKategori
    {
        public int ID { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int KategoriID { get; set; }
    }
}
