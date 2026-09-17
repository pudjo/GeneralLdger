using Accounting.DTO;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace Accounting.ViewModels.Inventory
{
    public class ProductListViewModel: BaseViewModel
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductListViewModel> _logger;
        public ObservableCollection<ProductDTO> Products { get; private set; } = new();
        public ProductDTO SelectedProduct { get; set; }

        public string SearchText { get; set; }


        // mode: when true the view shows "Pilih" button and acts as search dialog
        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set => SetProperty(ref _isSelectionMode, value);
        }

        public event EventHandler? RequestClose;

        public IRelayCommand CloseCommand { get; }
        public IRelayCommand AddCommand { get; }
        public IRelayCommand CloseFormCommand { get; }
        public IRelayCommand SearchCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<ProductDTO> DeleteCommand { get; }
        public IRelayCommand<ProductDTO> DetailCommand { get; }

        public IRelayCommand<ProductDTO> SelectCommand { get; }
        public event EventHandler<ProductDTO?>? OpenDetailRequested; // view handles opening detail window

        public IRelayCommand<ProductDTO> OpenDetailCommand { get; }

        public  ProductListViewModel(IProductService service, ILogger<ProductListViewModel> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            

            AddCommand = new RelayCommand(OnAdd);
            SearchCommand = new RelayCommand(async () => await OnSearchAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            CloseCommand = new RelayCommand(OnClose);
            OpenDetailCommand = new RelayCommand<ProductDTO>(OnOpenDetail);
            SelectCommand = new RelayCommand<ProductDTO>(OnSelect);
            DeleteCommand = new RelayCommand<ProductDTO>(async p => await OnDeleteAsync(p));
            DetailCommand = new RelayCommand<ProductDTO>(p => OnDetailRequested(p));
            // subscribe to product changes so the list refreshes when other parts of app add/update/delete
            _service.ProductsChanged += Service_ProductsChanged;

            _ = LoadAsync();
        }

        private void Service_ProductsChanged(object? sender, EventArgs e)
        {
            // reload on UI thread
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.InvokeAsync(async () => await LoadAsync().ConfigureAwait(false));
            }
            else
            {
                _ = LoadAsync();
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync().ConfigureAwait(false);

                // Update the existing ObservableCollection so bindings update automatically
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Products.Clear();
                        foreach (var p in list) Products.Add(p);
                    });
                }
                else
                {
                    Products.Clear();
                    foreach (var p in list) Products.Add(p);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load products failed");
            }
        }
        private void OnClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        private void OnDetailRequested(ProductDTO dto)
        {
            if (dto == null) return;
            OpenDetailRequested?.Invoke(this, dto);
        }



        private async Task OnSearchAsync()
        {
            try { 
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadAsync();
                return;
            }

            var results = await _service.SearchAsync(SearchText).ConfigureAwait(false);
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Products.Clear();
                    foreach (var p in results) Products.Add(p);
                });
            }
            else
            {
                Products.Clear();
                foreach (var p in results) Products.Add(p);
            }
        }
        catch (Exception ex)
        {
           _logger.LogError(ex, "Search products failed");
        }

     }
        private async Task OnDeleteAsync(ProductDTO? dto)
        {
            if (dto == null) return;

            // optional confirmation
            var result = MessageBox.Show($"Hapus produk '{dto.Name}' ?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var ok = await _service.DeleteAsync(dto.Id).ConfigureAwait(false);
                if (ok)
                {
                    // refresh list
                    await LoadAsync().ConfigureAwait(false);
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() => MessageBox.Show("Hapus gagal.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning));
                }
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher?.Invoke(() => MessageBox.Show($"Hapus gagal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private void OnSelect(ProductDTO dto)
        {
            if (dto == null) return;
            SelectedProduct = dto;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }



        private void OnAdd()
        {
            var dto = new ProductDTO();
            OpenDetail(dto);
        }

        private void OnOpenDetail(ProductDTO dto)
        {
            if (dto == null) return;
            OpenDetail(dto);
        }

        private void OpenDetail(ProductDTO dto)
        {
            var vm = new Accounting.ViewModels.Inventory.ProductDetailViewModel(_service, _logger, dto);
            var win = new Accounting.Views.Inventory.ProductDetailWindow
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
        // selection handler used when running as selection dialog

    }
}
