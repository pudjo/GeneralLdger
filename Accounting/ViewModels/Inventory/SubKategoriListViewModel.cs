using Accounting.DTO;
using Accounting.IRepositories.Inventory;
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
    internal class SubKategoriListViewModel : BaseViewModel
    {
        private readonly ISubKategoriService _service;
        private readonly IKategoriQueryRepository _kategoriQuery; // <-- added
        private readonly ILogger<SubKategoriListViewModel> _logger;

        private ObservableCollection<SubKategoriDTO> _items = new();
        public ObservableCollection<SubKategoriDTO> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private SubKategoriDTO _selectedItem;
        public SubKategoriDTO SelectedItem
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
        public IRelayCommand<SubKategoriDTO> OpenDetailCommand { get; }

        // Updated ctor to receive IKategoriQueryRepository
        public SubKategoriListViewModel(ISubKategoriService service,
                                        IKategoriQueryRepository kategoriQuery,
                                        ILogger<SubKategoriListViewModel> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _kategoriQuery = kategoriQuery ?? throw new ArgumentNullException(nameof(kategoriQuery));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AddCommand = new RelayCommand(OnAdd);
            SearchCommand = new RelayCommand(async () => await OnSearchAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            OpenDetailCommand = new RelayCommand<SubKategoriDTO>(OnOpenDetail);

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync().ConfigureAwait(false);
                Application.Current?.Dispatcher.Invoke(() => Items = new ObservableCollection<SubKategoriDTO>(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load SubKategori failed");
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
                    .Where(x => x.Nama?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                x.KategoriNama?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                Application.Current?.Dispatcher.Invoke(() => Items = new ObservableCollection<SubKategoriDTO>(results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search SubKategori failed");
            }
        }

        private void OnAdd()
        {
            var dto = new SubKategoriDTO();
            OpenDetail(dto);
        }

        private void OnOpenDetail(SubKategoriDTO dto)
        {
            if (dto == null) return;
            OpenDetail(dto);
        }

        private void OpenDetail(SubKategoriDTO dto)
        {
            // pass kategoriQuery so the detail VM can load KategoriList
            var vm = new SubKategoriDetailViewModel(dto, _service, _logger, _kategoriQuery);
            var win = new Accounting.Views.Inventory.SubKategoriDetailWindow
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
    }
}
    


