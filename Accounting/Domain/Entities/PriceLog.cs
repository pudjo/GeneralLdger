using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities
{
    internal class PriceLog
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime Tanggal { get; set; }   
        public decimal HargaJual { get; set; }  
        public int CreatedBy { get; set; }  
        

    }
}
