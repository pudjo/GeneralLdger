using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities
{
    internal class Jenis
    {
        public int ID{ get; set; }        // Contoh: AK-0101
        public string  Kode { get; set; }        // Contoh: AK-0101
        public string Nama { get; set; }        
        public int? ParentID { get; set; }

        // public string ParentKode { get; set; } // Operasional, Investasi, Pendanaan
        public int RootID { get; set; } // ID of the root category
        public bool Leaf { get; set; } // ID of the root category
    }
}
