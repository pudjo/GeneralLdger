
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Accounting.DTO
{
    public class CashFlowItemDTO : BaseViewModel
    {
        private string _code;
        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _parentCode;
        public string ParentCode
        {
            get => _parentCode;
            set { _parentCode = value; OnPropertyChanged(); }
        }

        private string _parentName;
        public string ParentName
        {
            get => _parentName;
            set { _parentName = value; OnPropertyChanged(); }
        }

        private string _groupType;
        public string GroupType
        {
            get => _groupType;
            set { _groupType = value; OnPropertyChanged(); }
        }

        // MENGGUNAKAN ObservableCollection AGAR UI MERESPON TAMBAH/HAPUS ANAK
        private ObservableCollection<CashFlowItemDTO> _children = new ObservableCollection<CashFlowItemDTO>();
        public ObservableCollection<CashFlowItemDTO> Children
        {
            get => _children;
            set { _children = value; OnPropertyChanged(); }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
