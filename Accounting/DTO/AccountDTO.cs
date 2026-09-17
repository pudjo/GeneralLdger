
namespace Accounting.DTO
{
    public class AccountDTO: BaseViewModel
    {
        
        private string _id;
        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Root { get; set; }
        

        private string _parentName;
        public string ParentName
        {
            get => _parentName;
            set { _parentName = value; OnPropertyChanged(); }
        }
        private string _idParent;
        public string IdParent
        {
            get => _idParent;
            set { _idParent = value; OnPropertyChanged(); }
        }
        
        public Single Leaf { get; set; }
        //public int Debet { get; set; }
        private int _debet;
        public int Debet
        {
            get => _debet;
            set { 
                _debet = value;
                BoolDebet = value == 1 ? true : false;
                OnPropertyChanged(); }
        }
        private bool _booldebet;
        public bool BoolDebet
        {
            get => _booldebet;
            set { _booldebet = value; OnPropertyChanged(); }
        }

        public List<AccountDTO> Children { get; set; } = new List<AccountDTO>();
        

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        private bool _isMatch;
        public bool IsMatch
        {
            get => _isMatch;
            set { _isMatch = value; OnPropertyChanged(); }
        }

    }
}
