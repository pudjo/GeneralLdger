using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO
{
    public  class LedgerRow
    {
        public string KodeAccount { get; set; }
        public string GroupId { get; set; }
        public decimal NilaiTahunIni { get; set; }
        public decimal SaldoAwal { get; set; }

        public LedgerRow(string kodeAccount, string groupId, decimal nilaiTahunIni, decimal saldoAwal)
        {
            KodeAccount = kodeAccount;
            GroupId = groupId;
            NilaiTahunIni = nilaiTahunIni;
            SaldoAwal = saldoAwal;
        }
    }
}
