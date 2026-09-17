
using MiniExcelLibs.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Accounting.Domain.Entities
{
    internal class JurnalImport : INotifyPropertyChanged
    {
        private DateTime _tanggal;
        private string _noAkun;
        private string _namaAkun;
        private string _noBukti;
        private string _keterangan;
        private decimal? _debit;
        private decimal? _kredit;

        public DateTime Tanggal
        {
            get => _tanggal;
            set { _tanggal = value; OnPropertyChanged(); }
        }

        [ExcelColumnName("No. Akun")]
        public string NoAkun
        {
            get => _noAkun;
            set { _noAkun = value; OnPropertyChanged(); }
        }

        [ExcelColumnName("Nama Akun")]
        public string NamaAkun
        {
            get => _namaAkun;
            set { _namaAkun = value; OnPropertyChanged(); }
        }

        [ExcelColumnName("No. Bukti")]
        public string NoBukti
        {
            get => _noBukti;
            set { _noBukti = value; OnPropertyChanged(); }
        }

        public string Keterangan
        {
            get => _keterangan;
            set { _keterangan = value; OnPropertyChanged(); }
        }

        public decimal? Debit
        {
            get => _debit;
            set { _debit = value; OnPropertyChanged(); }
        }

        public decimal? Kredit
        {
            get => _kredit;
            set { _kredit = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
