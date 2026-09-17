using Accounting.DTO;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
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

    internal class KategoriDetailViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly IKategoriService _service;
        private readonly ILogger _logger;

        // Single backing field + single public property Item
        private KategoriDTO _item;
        public KategoriDTO Item
        {
            get => _item;
            set
            {
                if (SetProperty(ref _item, value))
                {
                    // Notify that nested property changed so validation runs again
                    OnPropertyChanged(nameof(Item));
                    OnPropertyChanged(nameof(Item.Nama)); // helpful for some WPF versions
                }
            }
        }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public event EventHandler<bool> RequestClose;

        public KategoriDetailViewModel(IKategoriService service, ILogger logger, KategoriDTO item)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Item = item ?? new KategoriDTO();

            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            CancelCommand = new RelayCommand(OnCancel);
        }

        private async Task OnSaveAsync()
        {
            try
            {
                // Use aggregated validation before saving
                if (!string.IsNullOrWhiteSpace(this.Error))
                {
                    MessageBox.Show(this.Error, "Validasi gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Item.ID == 0)
                {
                    var id = await _service.CreateAsync(Item).ConfigureAwait(false);
                    if (id > 0)
                    {
                        Item.ID = id;
                        RequestClose?.Invoke(this, true);
                        return;
                    }
                }
                else
                {
                    var ok = await _service.UpdateAsync(Item).ConfigureAwait(false);
                    if (ok)
                    {
                        RequestClose?.Invoke(this, true);
                        return;
                    }
                }

                MessageBox.Show("Gagal menyimpan kategori.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save Kategori failed");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel() => RequestClose?.Invoke(this, false);

        // IDataErrorInfo: aggregate and per-property validation
        public string Error
        {
            get
            {
                if (Item == null) return string.Empty;
                if (string.IsNullOrWhiteSpace(Item.Nama)) return "Nama wajib diisi.";
                return string.Empty;
            }
        }

        //public string this[string columnName] => throw new NotImplementedException();

        /*    public string this[string columnName]
            {
                get
                {
                    if (Item == null) return string.Empty;

                    var simpleName = nameof(Item.Nama);             // "Nama"
                    var dottedName = $"{nameof(Item)}.{simpleName}"; // "Item.Nama"

                    if (columnName == simpleName || columnName == dottedName)
                    {
                        return string.IsNullOrWhiteSpace(Item.Nama) ? "Nama wajib diisi." : string.Empty;
                    }

                    return string.Empty;
                }
            }
        */
        [IndexerName("SistemValidasi")]
        public string this[string columnName]
        {
            get
            {
                if (Item == null) return string.Empty;

                // SEBELUMNYA: nameof(Item.Nama) -> ERROR
                // SEKARANG: Gunakan nama Class DTO-nya langsung
                var simpleName = nameof(KategoriDTO.Nama);             // Menghasilkan "Nama"
                var dottedName = $"{nameof(Item)}.{simpleName}";        // Menghasilkan "Item.Nama"

                if (columnName == simpleName || columnName == dottedName)
                {
                    return string.IsNullOrWhiteSpace(Item.Nama) ? "Nama wajib diisi." : string.Empty;
                }

                return string.Empty;
            }
        }

    }
}


    
