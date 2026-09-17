
namespace Accounting.Domain.Entities
{
    internal class Jurnal
    {
        public int Id{ get; set; }
        public DateTime JournalDate { get; set; }
        public string RefNo { get; set; }
        public string Description { get; set; }
        public decimal DebetAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public int Status { get; set; }
        public int ReferenceType { set; get; }
        public int AccountPeriode { set; get; }
        public int IDCRT { set; get; }
        public int Source { set; get; }
        public DateTime DCRT { set; get; }

        public List<JurnalDetail> Detail { set; get; } = new List<JurnalDetail>();

    }
}
