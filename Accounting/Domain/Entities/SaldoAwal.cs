

namespace Accounting.Domain.Entities
{
    internal class SaldoAwal
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string  AccountCode { get; set; }
        public decimal Debet { get; set; }  
        public decimal Credit { get; set; }

    }
}
