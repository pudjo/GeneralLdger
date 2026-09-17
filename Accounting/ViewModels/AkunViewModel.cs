

using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Menu;
using Accounting.Services.Accounts;
using Accounting.Views.Account;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels
{
    internal class AkunViewModel : BaseViewModel, IMenuItem
    {
        private ObservableCollection<AccountDTO> _akunTreeList;
        IAccountService service;
        public AkunViewModel(IAccountService _service)
        {
            service = _service;
            service.AccountsChanged += OnAccountChangedHandler;
            ExportCommand = new RelayCommand(async () => await ExportAsync());
            ImportCommand = new RelayCommand(async () => await ImportAsync());

            LoadSimulasiData();

        }
        private async void OnAccountChangedHandler(object? sender, EventArgs e)
        {

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
        public ObservableCollection<AccountDTO> AkunTreeList
        {
            get => _akunTreeList;
            set { _akunTreeList = value; OnPropertyChanged(); }
        }


        private async Task LoadSimulasiData()
        {
            try
            {
                List<AccountDTO> rawData = await service.GetTreeAccounts();

                _originalList = rawData; // Simpan master data
                if (AkunTreeList == null)
                {
                    AkunTreeList = new ObservableCollection<AccountDTO>();
                }

                AkunTreeList.Clear();
                foreach (var item in rawData)
                {
                    AkunTreeList.Add(item);
                }
            }
            catch (Exception ex)
            {
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


        public ICommand CopyID => new RelayCommand<AccountDTO>(akun =>
        {
            if (akun == null) return;

            // Copy text in WPF
            Clipboard.SetText(akun.Id);
        });
        public ICommand CopyName => new RelayCommand<AccountDTO>(akun =>
        {
            if (akun == null) return;

            Clipboard.SetText(akun.Name);
        });
        public ICommand ShowDetailCommand => new RelayCommand<AccountDTO>(akun =>
        {
            if (akun == null) return;


            var detailWindow = new DetailAkunWindow(akun);

            // Membuka secara Modal (User tidak bisa klik window utama sebelum ini ditutup)
            detailWindow.ShowDialog();
        });


        public string Title { get; } = "Account";

        // Perintah yang dipanggil dari Klik Kanan TreeView
        public ICommand AddChildCommand => new RelayCommand<AccountDTO>(async akun =>
        {
            if (akun == null) return;

            AccountDTO akubBaru = new AccountDTO();
            akubBaru.IdParent = akun.Id;
            akubBaru.ParentName = akun.Name;
            akubBaru.Name = "";
            akubBaru.Id = "";
            akubBaru.Root = akun.Root + 1;

            var detailWindow = new DetailAkunWindow(akubBaru, true);

            // Membuka secara Modal dan menunggu user menutup window tersebut
            bool? result = detailWindow.ShowDialog();

            // Lakukan reload data HANYA SETELAH window detail ditutup 
            // (Diasumsikan user jadi menyimpan data baru)
            await LoadSimulasiData();

            // Expand kembali parent yang sedang dikerjakan agar posisinya tetap terbuka
            SearchText = akun.Name;
            ExecuteSearch(); // Memanggil search/expand logic agar parent-nya terbuka otomatis
        });

        public ICommand DeleteCommand => new AsyncRelayCommand<AccountDTO>(async akun =>
        {
            if (akun == null) return;

            // 1. Simpan ID Parent sebelum akun dihapus
            string parentId = akun.IdParent;

            int countChildren = await service.GetChildrenCount(akun.Id);
            if (countChildren > 0)
            {
                MessageBox.Show("Akun ini memiliki anak, tidak bisa dihapus.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 2. Lakukan proses hapus
            bool result = await service.DeleteAccountAsync(akun.Id);

            if (result)
            {
                MessageBox.Show("Akun berhasil dihapus", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Gagal menghapus akun.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Muat ulang data (karena menggunakan .Clear() dan .Add(), tree tidak akan collapse total)
            await LoadSimulasiData();

            // 4. Cari kembali parent berdasarkan ID-nya, lalu set IsExpanded dan IsSelected menjadi true
            if (!string.IsNullOrEmpty(parentId))
            {
                var parentNode = FindNodeById(_originalList, parentId);
                if (parentNode != null)
                {
                    parentNode.IsExpanded = true;
                    parentNode.IsSelected = true;

                    // Jika ingin memastikan hierarki di atasnya ikut terbuka
                    ExpandParents(parentNode);
                }
            }
        });
        private AccountDTO FindNodeById(IEnumerable<AccountDTO> items, string targetId)
        {
            foreach (var item in items)
            {
                if (item.Id == targetId) return item;

                if (item.Children != null && item.Children.Any())
                {
                    var found = FindNodeById(item.Children, targetId);
                    if (found != null) return found;
                }
            }
            return null;
        }
        #region serching
        // searching
        private string _searchText;
        private List<AccountDTO> _originalList; // Menyimpan data asli dari database

        // Properti untuk Binding ke TextBox
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // Command Seach button
        public ICommand SearchCommand => new RelayCommand(() => ExecuteSearch());
        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ResetHighlight(_originalList);
                return;
            }

            string term = SearchText.ToLower();
            ApplyHighlight(_originalList, term);

        }
        private void ApplyHighlight(IEnumerable<AccountDTO> items, string term)
        {
            foreach (var item in items)
            {
                // Cek apakah item ini cocok
                item.IsMatch = item.Name.ToLower().Contains(term) || item.Id.ToLower().Contains(term);

                // Jika cocok, kita expand parent-nya agar terlihat
                if (item.IsMatch)
                {
                    ExpandParents(item);
                }

                if (item.Children != null && item.Children.Any())
                {
                    ApplyHighlight(item.Children, term);
                }
            }
        }

        private void ResetHighlight(IEnumerable<AccountDTO> items)
        {
            foreach (var item in items)
            {
                item.IsMatch = false;
                if (item.Children != null) ResetHighlight(item.Children);
            }
        }

        private void ExpandParents(AccountDTO item)
        {
            if (item == null) return;

            // Kita cari siapa orang tua dari item ini di dalam master data (_originalList)
            var parent = FindParent(_originalList, item.Id);

            if (parent != null)
            {
                parent.IsExpanded = true;
                // Rekursif ke atas sampai ketemu Root
                ExpandParents(parent);
            }
        }

        private AccountDTO FindParent(IEnumerable<AccountDTO> items, string childId)
        {
            foreach (var item in items)
            {
                if (item.Children != null && item.Children.Any(c => c.Id == childId))
                {
                    return item;
                }

                if (item.Children != null)
                {
                    var found = FindParent(item.Children, childId);
                    if (found != null) return found;
                }
            }
            return null;
        }


        #endregion

        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }


        private async Task ExportAsync()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"Accounts_{DateTime.Now:yyyyMMdd}.xlsx",
                    DefaultExt = "xlsx"
                };
                if (dlg.ShowDialog() == true)
                {
                    var path = dlg.FileName;
                    await service.ExportAccountsToExcelAsync(path);
                    MessageBox.Show($"Export berhasil: {path}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ImportAsync()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    
                    Multiselect = false
                };
                if (dlg.ShowDialog() == true)
                {
                    var path = dlg.FileName;
                    var count = await service.ImportAccountsFromExcelAsync(path);
                    MessageBox.Show($"Import selesai. {count} record tersimpan.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);

                    // reload list (sesuaikan method reload di ViewModel Anda)
                    if (this.GetType().GetMethod("LoadAccountsAsync") is System.Reflection.MethodInfo mi)
                    {
                        var t = mi.Invoke(this, null) as Task;
                        if (t != null) await t;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error import: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
