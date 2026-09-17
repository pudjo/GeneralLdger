
namespace Accounting.DTO
{
    public class GeneralLedgerLaporanDTO
    {
        public string ID{ set; get; }        
        public string AccountCode { set; get; }
        public string AccountName { set; get; }
        public decimal CurrentAmount { set; get; }
        public decimal BeforeAmount { set; get; }
        public int Root  { set; get; }
        
    }
}
