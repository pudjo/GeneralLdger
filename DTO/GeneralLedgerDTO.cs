

namespace Accounting.DTO
{
    public class GeneralLedgerDTO
    {
        public int Id { set; get; }
        public int JournalHeaderId { set; get; }
        public DateTime TrxDate { set; get; }
        public string RefNo { set; get; }
        public string Description { set; get; }
        public string AccountCode { set; get; }

        // Pindahkan ke sini agar Dapper langsung mengenali kolom ke-6 dan ke-7
        public decimal Debet { set; get; }
        public decimal Credit { set; get; }
        public decimal SALDOAWALDebet { set; get; }
        public decimal SALDOAWALCredit { set; get; }
        public string AccountName { set; get; }

        // 2. Properti tambahan yang tidak ada di SQL SELECT awal
        public decimal Saldo { set; get; }
        public int Root { set; get; }

        // 3. Properti Logika Akuntansi (Gunakan get/set tradisional agar aman di .NET 8)
        
    }
}
