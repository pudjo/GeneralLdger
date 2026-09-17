
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Services.Accounts;

using System.Collections.ObjectModel;
using System.Windows;

namespace Accounting.ViewModels.Akun
{
    internal class AkunSelectionWindowVewModel:BaseViewModel
    {
        
        private ObservableCollection<AccountDTO> _akunTreeList;
        IAccountService service;
        
        private AccountDTO _selectedAkun;
        public AccountDTO SelectedAkun
        {
            get => _selectedAkun;
            set
            {
                    _selectedAkun = value;
                    OnPropertyChanged();

            }
        }
        public AccountDTO GetSelectedAccount(IEnumerable<AccountDTO> items)
        {
            foreach (var item in items)
            {
                if (item.IsSelected) return item;
                if (item.Children != null && item.Children.Any())
                {
                    var found = GetSelectedAccount(item.Children);
                    if (found != null) return found;
                }
            }
            return null; // Tidak ada yang dipilih
        }

        public  AkunSelectionWindowVewModel(IAccountService _service)
        {
            service = _service;
            service.AccountsChanged += OnAccountChangedHandler;
            _= LoadSimulasiData();
        }
        //public Ob
        //servableCollection<ProductDTO> Products { get; private set; } = new();
        public ObservableCollection<AccountDTO> AkunTreeList { get; private set; } = new();

       
        private async void OnAccountChangedHandler(object? sender, EventArgs e)
        {
            // Pastikan eksekusi berada di UI thread jika aplikasi menggunakan Dispatcher (misal: WPF)
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadSimulasiData();
                });
            }
            else
            {
                await LoadSimulasiData();
            }
        }
        
        private async Task LoadSimulasiData()
        {
         
            try
            {
                List<AccountDTO> rawData = await service.GetTreeAccounts();         
                AkunTreeList = new ObservableCollection<AccountDTO>(rawData);

            }
            catch (Exception ex)
            {
                // Log tetap dicatat di latar belakang
                // Tampilkan pesan ke User
                MessageBox.Show($"Fail get data: {ex.Message}", "Error Sistem",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        

        bool _isExpanded;
        bool _isSelected;

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
                
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

    }

}

