
using CommunityToolkit.Mvvm.ComponentModel;

namespace Accounting.DTO
{
    internal class JurnalDTO : ObservableObject
    
    {
        public int Id { set; get; }
        public string RefNo { set; get; }
        public DateTime JournalDate { set; get; }
        private string _description;
        public string Description
        {
            set => SetProperty(ref _description, value);
            get => _description;
        }
        public int Status { set; get; }


        public decimal TotalDebet { set; get; }
        public decimal TotalCredit { set; get; }

        public List<JournalDetailDTO> Detail { set; get; } = new List<JournalDetailDTO>();

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
