using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO
{
    internal class SubKategoriDTO
    {
        public int ID { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int KategoriID { get; set; }

        // Optional: friendly name for UI display
        public string KategoriNama { get; set; } = string.Empty;

    }
}
