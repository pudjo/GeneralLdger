using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO
{
    internal class JenisDTO
    {
        public int ID { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int? ParentID { get; set; }
        public string Kode { get; set; }
        // UI convenience
        public string ParentNama { get; set; } = string.Empty;

        // For tree building
        public List<JenisDTO> Children { get; set; } = new List<JenisDTO>();

    }
}
