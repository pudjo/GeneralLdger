
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Accounting.DTO
{
    internal class SaldoAwalDTO:BaseViewModel
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        private decimal _debet;
        private decimal _credit;
        public decimal Debet
        {
            get => _debet;
            set
            {
                if (_debet != value)
                {
                    _debet = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Credit
        {
            get => _credit;
            set
            {
                if (_credit != value)
                {
                    _credit = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Implementasi INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}
