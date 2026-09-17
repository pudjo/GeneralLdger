using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO.AP
{
    internal class PembelianDTO
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime Tanggal { get; set; }
        public int VendorID
        {
            get; set;
        }
        public string VendorName { get; set; }
        public int StatusID
        {
            get; set;
        }
        
        public List<PembelianDetailDTO> Details { get; set; } = new List<PembelianDetailDTO>();
    }
}
