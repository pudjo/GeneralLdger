using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities.AR
{
    internal class PenjualanAddress
    {
        public int Id { get; set; }
        public int PenjualanId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string NoHP { get; set; }
        public string Email { get; set; }
    }
}
