using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities.AP
{
    internal class PembelianDetail
    {
        public int Id { get; set; }
        public int PembelianId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        
        public decimal Discount { get; set; }
        public decimal Total => Quantity * Price;
    }
}
