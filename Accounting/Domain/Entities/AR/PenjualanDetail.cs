using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities.AR
{
    internal class PenjualanDetail
    {
        public int Id { get; set; }
        public int PenjualanId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SellingPrice { get; set; }

        public decimal Total => Quantity * Price;
    }
}
