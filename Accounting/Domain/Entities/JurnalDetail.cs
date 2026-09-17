
namespace Accounting.Domain.Entities
{
    internal class JurnalDetail
    {
        public int Id { set; get; }
        public int JournalHeaderId { set; get; }
        public string AccountCode { set; get; }
        public decimal Debet { set; get; }
        public decimal Credit { set; get; }

    }
}
