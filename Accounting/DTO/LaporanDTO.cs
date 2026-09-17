
namespace Accounting.DTO
{
    public class LaporanDTO
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }

        // Saldo Awal (Tahun Lalu)
        public decimal SaldoAwalDebet { get; set; }
        public decimal SaldoAwalKredit { get; set; }
        public decimal TotalSaldoAwal => SaldoAwalDebet - SaldoAwalKredit; // Sesuaikan aturan saldo normal

        // Buku Besar (Tahun Ini)
        public decimal TahunIniDebet { get; set; }
        public decimal TahunIniKredit { get; set; }
        public decimal TotalTahunIni => TahunIniDebet - TahunIniKredit; // Sesuaikan aturan saldo normal
    }
}
