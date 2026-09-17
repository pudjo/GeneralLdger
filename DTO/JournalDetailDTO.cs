
namespace Accounting.DTO
{
    internal class JournalDetailDTO : BaseViewModel

    {
        public int IDDetail { set; get; }
        public int JournalHeaderId { set; get; }
        
        public string RefNo { set; get; }

        

        private decimal _debet;
        public decimal Debet
        {
            get => _debet;
            set
            {
                _debet = value;
                OnPropertyChanged(nameof(Debet));
            }
        }

        private decimal _credit;
        public decimal Credit
        {
            get => _credit;
            set
            {
                _credit = value;
                OnPropertyChanged(nameof(Credit));
            }
        }

        private string _accountCode;
        public string AccountCode
        {
            get => _accountCode;
            set => SetProperty(ref _accountCode, value); // Ini yang memicu UI update
        }

        private string _accountName;
        public string AccountName
        {
            get => _accountName;
            set => SetProperty(ref _accountName, value);
        }
    }
}
