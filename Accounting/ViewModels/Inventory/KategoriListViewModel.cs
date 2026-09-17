using Accounting.DTO;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.Inventory
{
    internal class KategoriListViewModel : BaseViewModel
    {
        private readonly IKategoriService _service;
        private readonly ILogger<KategoriListViewModel> _logger;

        // BACKING FIELD and proper SetProperty so UI is notified
        private ObservableCollection<KategoriDTO> _items = new();
        public ObservableCollection<KategoriDTO> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private KategoriDTO _selectedItem;
        public KategoriDTO SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public IRelayCommand AddCommand { get; }
        public IRelayCommand SearchCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<KategoriDTO> OpenDetailCommand { get; }

        public KategoriListViewModel(IKategoriService service, ILogger<KategoriListViewModel> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AddCommand = new RelayCommand(OnAdd);
            SearchCommand = new RelayCommand(async () => await OnSearchAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            OpenDetailCommand = new RelayCommand<KategoriDTO>(OnOpenDetail);

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync().ConfigureAwait(false);

                // Update on UI thread. Use SetProperty so binding is notified.
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    // Option A: replace the collection instance (fires PropertyChanged)
                    Items = new ObservableCollection<KategoriDTO>(list);

                    // Option B (alternative): keep same instance and update items:
                    // _items.Clear();
                    // foreach (var it in list) _items.Add(it);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load Kategori failed");
            }
        }

        private async Task OnSearchAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadAsync();
                    return;
                }

                var results = (await _service.GetAllAsync().ConfigureAwait(false))
                    .Where(x => x.Nama?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Items = new ObservableCollection<KategoriDTO>(results);
                    // Or update existing:
                    // _items.Clear();
                    // foreach (var it in results) _items.Add(it);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search Kategori failed");
            }
        }
        private void OnAdd()
        {
            var dto = new KategoriDTO();
            OpenDetail(dto);
        }

        private void OnOpenDetail(KategoriDTO dto)
        {
            if (dto == null) return;
            OpenDetail(dto);
        }

        private void OpenDetail(KategoriDTO dto)
        {
            var vm = new KategoriDetailViewModel(_service, _logger, dto);
            var win = new Accounting.Views.Inventory.KategoriDetailWindow
            {
                DataContext = vm,
                Owner = Application.Current?.MainWindow
            };

            vm.RequestClose += async (s, saved) =>
            {
                win.DialogResult = saved;
                win.Close();
                if (saved) await LoadAsync();
            };

            win.ShowDialog();
        }



        // ... rest of class unchanged ...
    }
}

