using Accounting.Domain;
using Accounting.Menu;
using Accounting.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
namespace Accounting
{

    public class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<IMenuItem> MenuItems { get; }

        private object? _selectedItem;
        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    IsMenuOpen = false;
                }
            }
        }
        private int selectedYear;
        public int SelectedYear
        {
            get => selectedYear;
            set => SetProperty(ref selectedYear, value);
        }
        private bool isMenuOpen;
        public bool IsMenuOpen
        {
            get => isMenuOpen;
            set => SetProperty(ref isMenuOpen, value);
        }

        public MainWindowViewModel()
        {
            MenuItems = new()
            {
                
                
    
            };
            SelectedYear = AppSession.SelectedYear;

            
        }
    }
}
