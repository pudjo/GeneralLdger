using Accounting.DTO;
using Accounting.Menu;
using Accounting.Services.Accounts;
using Accounting.Services.GeneralLedgerService;
using Accounting.Views.GeneralLedger;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;

namespace Accounting.ViewModels.GeneralLedger
{
    internal class CashFlowItemViewModel : BaseViewModel, IMenuItem
    {
        private ObservableCollection<CashFlowItemDTO> _cashFlowTreeList;
        CashFlowService service;
        public CashFlowItemViewModel(CashFlowService _service)
        {
            service = _service;
            service.CashFlowChanged += OnCashFlowChangedHandler;
            LoadTreeData();

        }
        // Handler event yang bertugas memicu reload pohon
        private async void OnCashFlowChangedHandler(object? sender, EventArgs e)
        {
            // Pastikan eksekusi UI thread jika diperlukan, atau langsung panggil LoadTreeData()
            await LoadTreeData();
        }
        public ObservableCollection<CashFlowItemDTO> CashFlowTreeList
        {
            get => _cashFlowTreeList;
            set { _cashFlowTreeList = value; OnPropertyChanged(); }
        }
        private async Task LoadTreeData()
        {
            try
            {
                List<CashFlowItemDTO> rawData = await service.GetTreeCashFlowItems();

                _originalList = rawData; // Simpan master data
                CashFlowTreeList = new ObservableCollection<CashFlowItemDTO>(rawData);
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


        public ICommand CopyID => new RelayCommand<CashFlowItemDTO>(akun =>
        {
            if (akun == null) return;

            // Copy text in WPF
            Clipboard.SetText(akun.Code);
        });
        public ICommand CopyName => new RelayCommand<CashFlowItemDTO>(akun =>
        {
            if (akun == null) return;

            Clipboard.SetText(akun.Name);
        });
        public ICommand ShowDetailCommand => new RelayCommand<CashFlowItemDTO>(cashFlowItem =>
        {
            if (cashFlowItem == null) return;

            // open detail window and pass callback to reload tree when saved/changed
            var wnd = new Views.GeneralLedger.DetailCashFlowWindow(cashFlowItem, service, () =>
            {
                // Fire-and-forget reload; UI thread is fine because LoadTreeData uses async/await
                _ = LoadTreeData();
            });

            wnd.Owner = System.Windows.Application.Current.MainWindow;
            wnd.ShowDialog();
        });



        public string Title { get; } = "Arus Kas";

        // Perintah yang dipanggil dari Klik Kanan TreeView
        public ICommand AddChildCommand => new RelayCommand<CashFlowItemDTO>(cashFlowItem =>
        {
            if (cashFlowItem == null) return;
            CashFlowItemDTO cashFlowBaru = new CashFlowItemDTO();
            cashFlowBaru.ParentCode= cashFlowItem.Code;
            cashFlowBaru.ParentName = cashFlowItem.Name;
            cashFlowBaru.Name = "";
            cashFlowBaru.Code= "";

            var wnd = new DetailCashFlowWindow(cashFlowBaru, this.service);
	wnd.Owner = Application.Current.MainWindow;
	wnd.ShowDialog();


            //var detailWindow = new DetailAkunWindow(akubBaru, true);
            LoadTreeData();
            SearchText = cashFlowItem.Name;


            // Membuka secara Modal (User tidak bisa klik window utama sebelum ini ditutup)
            //detailWindow.ShowDialog();


        });
        #region serching
        // searching
        private string _searchText;
        private List<CashFlowItemDTO> _originalList;

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
        private void ApplyHighlight(IEnumerable<CashFlowItemDTO> items, string term)
        {
            foreach (var item in items)
            {
                // Cek apakah item ini cocok
                item.IsMatch = item.Name.ToLower().Contains(term) || item.Name.ToLower().Contains(term);

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

        private void ResetHighlight(IEnumerable<CashFlowItemDTO> items)
        {
            foreach (var item in items)
            {
                item.IsMatch = false;
                if (item.Children != null) ResetHighlight(item.Children);
            }
        }

        private void ExpandParents(CashFlowItemDTO item)
        {
            if (item == null) return;

            // Kita cari siapa orang tua dari item ini di dalam master data (_originalList)
            var parent = FindParent(_originalList, item.Code);

            if (parent != null)
            {
                parent.IsExpanded = true;
                // Rekursif ke atas sampai ketemu Root
                ExpandParents(parent);
            }
        }

        private CashFlowItemDTO FindParent(IEnumerable<CashFlowItemDTO> items, string childId)
        {
            foreach (var item in items)
            {
                if (item.Children != null && item.Children.Any(c => c.Code == childId))
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

        public void Dispose()
        {
            if (service != null)
            {
                // Unsubscribe dari event agar tidak terjadi memory leak
                service.CashFlowChanged -= OnCashFlowChangedHandler;
            }
        }
        #endregion
    }
}
