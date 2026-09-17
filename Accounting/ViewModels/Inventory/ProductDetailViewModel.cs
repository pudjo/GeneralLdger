using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.Inventory
{
    internal class ProductDetailViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly IProductService _service;
        private readonly ILogger _logger;

        public ProductDTO Product { get; set; } // keep original property name if used elsewhere
        public ProductDTO Item { get => Product; set => Product = value; } // convenience

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IRelayCommand SearchJenisCommand { get; } // NEW

        public event EventHandler<bool> RequestClose;

        public ProductDetailViewModel(IProductService service, ILogger logger, ProductDTO product)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Product = product ?? new ProductDTO();

            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            CancelCommand = new RelayCommand(OnCancel);
            SearchJenisCommand = new RelayCommand(OpenJenisPicker); // initialize
        }

        private async Task OnSaveAsync()
        {
            try
            {
                // Basic validation example
                if (string.IsNullOrWhiteSpace(Product.Name))
                {
                    MessageBox.Show("Nama produk wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Product.Id == 0)
                {
                    var id = await _service.CreateAsync(Product, Product.CreatedBy ?? "system").ConfigureAwait(false);
                    if (id > 0)
                    {
                        Product.Id = id;
                        RequestClose?.Invoke(this, true);
                        return;
                    }
                }
                else
                {
                    var ok = await _service.UpdateAsync(Product, Product.CreatedBy ?? "system").ConfigureAwait(false);
                    if (ok)
                    {
                        RequestClose?.Invoke(this, true);
                        return;
                    }
                }

                MessageBox.Show("Gagal menyimpan produk.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save product failed");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel() => RequestClose?.Invoke(this, false);

        // NEW: Open Jenis picker window and set selected Jenis into Item.Jenis*
        private void OpenJenisPicker()
        {
            try
            {
                // Resolve required services/VM from DI
                var jenisService = App.ServiceProvider.GetRequiredService<IJenisService>();
                var jenisQuery = App.ServiceProvider.GetRequiredService<IJenisQueryRepository>();
                var jenisLogger = App.ServiceProvider.GetRequiredService<ILogger<Accounting.ViewModels.Inventory.JenisTreeViewModel>>();

                var jenisVm = new Accounting.ViewModels.Inventory.JenisTreeViewModel(jenisService, jenisQuery, jenisLogger);

                var picker = new Accounting.Views.Inventory.JenisPickerWindow
                {
                    DataContext = jenisVm,
                    Owner = Application.Current?.MainWindow
                };

                // Show dialog; when closed with true, use selected item
                var result = picker.ShowDialog();
                if (result == true && jenisVm.Selected != null)
                {
                    Item.Jenis = jenisVm.Selected.ID;
                    Item.JenisNama = jenisVm.Selected.Nama;
                    // notify UI that Item.JenisNama changed
                    OnPropertyChanged(nameof(Item));
                    OnPropertyChanged(nameof(Item.JenisNama));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenJenisPicker failed");
            }
        }

        // IDataErrorInfo minimal
        public string Error => string.Empty;
        [IndexerName("SistemValidasi")]
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(Product.Name) && string.IsNullOrWhiteSpace(Product.Name)) return "Nama wajib diisi.";
                return string.Empty;
            }
        }
    }

}