using Accounting.DTO;
using Accounting.Services.Inventory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.Controls
{
    internal class ProductComboViewModel: BaseViewModel
    {
      
        private IProductService? _service;
        private int? _jenisId;

        public ObservableCollection<ProductDTO> Products { get; } = new ObservableCollection<ProductDTO>();

        private int? _selectedProductId;
        public int? SelectedProductId
        {
            get => _selectedProductId;
            set
            {
                if (_selectedProductId == value) return;
                _selectedProductId = value;
                OnPropertyChanged(nameof(SelectedProductId));

                SelectedProduct = Products.FirstOrDefault(p => p.Id == _selectedProductId);
            }
        }

        private ProductDTO? _selectedProduct;
        public ProductDTO? SelectedProduct
        {
            get => _selectedProduct;
            private set
            {
                if (_selectedProduct == value) return;
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetService(IProductService? service)
        {
            if (_service != null)
                _service.ProductsChanged -= Service_ProductsChanged;

            _service = service;

            if (_service != null)
            {
                _service.ProductsChanged += Service_ProductsChanged;
                _ = LoadAsync();
            }
            else
            {
                Application.Current?.Dispatcher?.Invoke(() => Products.Clear());
            }
        }

        private void Service_ProductsChanged(object? sender, EventArgs e)
            => Application.Current?.Dispatcher?.InvokeAsync(async () => await LoadAsync().ConfigureAwait(false));

        public async Task LoadAsync()
        {
            if (_service == null) return;

            try
            {
                var list = _jenisId.HasValue ? await _service.GetByJenisAsync(_jenisId.Value).ConfigureAwait(false)
                                             : await _service.GetAllAsync().ConfigureAwait(false);

                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Products.Clear();
                        foreach (var p in list) Products.Add(p);

                        if (_selectedProductId.HasValue)
                            SelectedProduct = Products.FirstOrDefault(x => x.Id == _selectedProductId.Value);
                    });
                }
                else
                {
                    Products.Clear();
                    foreach (var p in list) Products.Add(p);
                    if (_selectedProductId.HasValue)
                        SelectedProduct = Products.FirstOrDefault(x => x.Id == _selectedProductId.Value);
                }
            }
            catch
            {
                // log optionally
            }
        }

        public void SetJenis(int? jenisId)
        {
            _jenisId = jenisId;
            _ = LoadAsync();
        }
    }
}
