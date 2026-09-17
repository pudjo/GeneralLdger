

namespace Accounting.Domain.Entities
{
    internal class GeneralLedger
    {
        public string RefNo { set; get; }
        public int JournalHeaderId { set; get; }
        public DateTime TrxDate { set; get; }
        public string Description { set; get; }
        public  decimal Debet { set; get; }
        public decimal Credit { set; get; }
        public string AccountCode { set; get; }
        public int Status { set; get; }
        public int  Source { set; get; }
        public int  ID { set; get; }



    }
}
