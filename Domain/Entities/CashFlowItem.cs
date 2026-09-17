using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities
{
    public class CashFlowItem
    {
        public string Code { get; set; }        // Contoh: AK-0101
        public string Name { get; set; }        // Contoh: Penerimaan dari Pelanggan
        public string ParentCode { get; set; } // Operasional, Investasi, Pendanaan
        public string GroupType { get; set; }
    }
}
