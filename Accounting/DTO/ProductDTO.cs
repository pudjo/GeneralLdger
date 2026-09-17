using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO
{
    public  class ProductDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal CurrentSellingPrice { get; set; }
        public int CurrentStock { get; set; }

        // NEW: Jenis reference
        public int Jenis { get; set; }
        public string JenisNama { get; set; } = string.Empty;

        // Optional audit fields if used elsewhere
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;

    }
}
