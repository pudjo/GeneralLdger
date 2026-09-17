using Accounting.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Domain.Entities
{
    internal class Contact
    {
        // Primary Key
        public int Id { get; set; }

        // Kode Unik (Contoh: VND-0001, CST-0025)
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // Memanfaatkan Enum yang sudah dibuat di atas
        public ContactType Type { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Nomor NPWP / Pajak
        public string TaxId { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Audit Fields (Opsional, tapi sangat disarankan untuk sistem ERP/Akuntansi)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
