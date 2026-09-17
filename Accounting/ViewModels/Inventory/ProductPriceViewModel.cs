using Accounting.Domain;
using Accounting.IRepositories;
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
    internal class ProductPriceViewModel : BaseViewModel
    {
        private readonly IProductQueryRepository _productQuery;
        private readonly IPriceLogService _priceLogService;
        private readonly ILogger<ProductPriceViewModel> _logger;

        private ObservableCollection<ProductPriceRow> _items = new();
        public ObservableCollection<ProductPriceRow> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public IRelayCommand RefreshCommand { get; }

        public ProductPriceViewModel(IProductQueryRepository productQuery, IPriceLogService priceLogService, ILogger<ProductPriceViewModel> logger)
        {
            _productQuery = productQuery ?? throw new ArgumentNullException(nameof(productQuery));
            _priceLogService = priceLogService ?? throw new ArgumentNullException(nameof(priceLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var products = await _productQuery.GetAllAsync().ConfigureAwait(false);
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Items = new ObservableCollection<ProductPriceRow>(
                        products.Select(p => new ProductPriceRow(p, SaveRowAsync)));
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load products failed");
            }
        }

        // Handler invoked by each row's SaveCommand
        private async Task SaveRowAsync(ProductPriceRow row)
        {
            try
            {
                if (row == null) return;

                // simple validation
                if (row.EditablePrice < 0)
                {
                    MessageBox.Show("Harga jual tidak boleh negatif.", "Validasi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Use AppSession or pass createdBy; fallback 0
                int createdBy = 0;
                try
                {
                    createdBy = int.Parse(AppSession.CurrentUser?.UserID ?? "0");
                }
                catch { createdBy = 0; }

                var ok = await _priceLogService.SavePriceAsync(row.Product.Id, row.EditablePrice, createdBy).ConfigureAwait(false);
                if (ok)
                {
                    // reflect updated price in DTO
                    row.Product.CurrentSellingPrice = row.EditablePrice;

                    // Notify UI about product change from the row itself
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        row.NotifyProductChanged();
                    });

                    _logger.LogInformation("Price saved for ProductId={Id}", row.Product.Id);
                }

                else
                {
                    MessageBox.Show("Gagal menyimpan harga.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveRowAsync failed for ProductId={Id}", row?.Product?.Id);
                MessageBox.Show($"Error: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

    }
}
