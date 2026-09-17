using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.Inventory
{
    internal class SubKategoriDetailViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly ISubKategoriService _service;
        private readonly IKategoriQueryRepository _kategoriQuery; // <-- expect injected repo
        private readonly ILogger _logger;

        private SubKategoriDTO _item;
        public SubKategoriDTO Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        private ObservableCollection<Accounting.DTO.KategoriDTO> _kategoriList = new();
        public ObservableCollection<Accounting.DTO.KategoriDTO> KategoriList
        {
            get => _kategoriList;
            set => SetProperty(ref _kategoriList, value);
        }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public event EventHandler<bool> RequestClose;

        // Updated ctor: require IKategoriQueryRepository
        public SubKategoriDetailViewModel(SubKategoriDTO item,
                                          ISubKategoriService service,
                                          ILogger logger,
                                          IKategoriQueryRepository kategoriQuery)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _kategoriQuery = kategoriQuery ?? throw new ArgumentNullException(nameof(kategoriQuery));

            Item = item ?? new SubKategoriDTO();

            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            CancelCommand = new RelayCommand(OnCancel);

            _ = LoadKategoriAsync();
        }

        private async Task LoadKategoriAsync()
        {
            try
            {
                var list = await _kategoriQuery.GetAllAsync().ConfigureAwait(false);
                Application.Current?.Dispatcher.Invoke(() => KategoriList = new ObservableCollection<Accounting.DTO.KategoriDTO>(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load kategori list failed");
            }
        }

        private async Task OnSaveAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    MessageBox.Show(Error, "Validasi gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                MessageBox.Show("Gagal menyimpan subkategori.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save SubKategori failed");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel() => RequestClose?.Invoke(this, false);

        public string Error
        {
            get
            {
                if (Item == null) return string.Empty;
                if (string.IsNullOrWhiteSpace(Item.Nama)) return "Nama wajib diisi.";
                if (Item.KategoriID <= 0) return "Kategori harus dipilih.";
                return string.Empty;
            }
        }


        [IndexerName("SistemValidasi")]
        public string this[string columnName]
        {
            get
            {
                if (Item == null) return string.Empty;
                var simpleNama = nameof(Item.Nama); // "Nama"
                var dottedNama = $"{nameof(Item)}.{simpleNama}"; // "Item.Nama"
                if (columnName == simpleNama || columnName == dottedNama)
                {
                    return string.IsNullOrWhiteSpace(Item.Nama) ? "Nama wajib diisi." : string.Empty;
                }

                var simpleKategori = nameof(Item.KategoriID); // "KategoriID"
                var dottedKategori = $"{nameof(Item)}.{simpleKategori}";
                if (columnName == simpleKategori || columnName == dottedKategori)
                {
                    return Item.KategoriID <= 0 ? "Kategori harus dipilih." : string.Empty;
                }

                return string.Empty;
            }
        }
    }
}

