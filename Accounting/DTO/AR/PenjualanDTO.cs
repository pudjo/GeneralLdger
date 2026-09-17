using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO.AR
{
    public class PenjualanDTO
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime Tanggal { get; set; }
        public int CutsID
        {
            get; set;
        }
        public string CUstomerName { get; set; }
        public int StatusID
        {
            get; set;
        }
        public int AddressId { get; set; }
        
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string NoHP { get; set; }
        public string Email { get; set; }
        public List<PenjualanDetailDTO> Details { get; set; } = new List<PenjualanDetailDTO>();
    }
}
