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
    internal class JenisDetailViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly IJenisService _service;
        private readonly IJenisQueryRepository _queryRepo;
        private readonly ILogger _logger;

        private JenisDTO _item;
        public JenisDTO Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        // parent selection list
        private ObservableCollection<JenisDTO> _parentList = new();
        public ObservableCollection<JenisDTO> ParentList
        {
            get => _parentList;
            set => SetProperty(ref _parentList, value);
        }

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public event EventHandler<bool> RequestClose;

        public JenisDetailViewModel(JenisDTO item, IJenisService service, IJenisQueryRepository queryRepo, ILogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _queryRepo = queryRepo ?? throw new ArgumentNullException(nameof(queryRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Item = item ?? new JenisDTO();

            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            CancelCommand = new RelayCommand(OnCancel);

            _ = LoadParentsAsync();
        }

        private async Task LoadParentsAsync()
        {
            try
            {
                var all = await _queryRepo.GetAllAsync().ConfigureAwait(false);
                // exclude self when editing to avoid circular parent
                var parents = all.Where(x => x.ID != Item.ID).ToList();
                Application.Current?.Dispatcher.Invoke(() => ParentList = new ObservableCollection<JenisDTO>(parents));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load parents failed");
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
                    var newId = await _service.CreateAsync(Item).ConfigureAwait(false);
                    if (newId > 0)
                    {
                        Item.ID = newId;
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

                MessageBox.Show("Gagal menyimpan jenis.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save Jenis failed");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel() => RequestClose?.Invoke(this, false);

        // IDataErrorInfo
        public string Error
        {
            get
            {
                if (Item == null) return string.Empty;
                if (string.IsNullOrWhiteSpace(Item.Nama)) return "Nama wajib diisi.";
                return string.Empty;
            }
        }
        [IndexerName("SistemValidasi")]
        public string this[string columnName]
        {
            get
            {
                if (Item == null) return string.Empty;
                var simpleNama = nameof(Item.Nama);
                var dottedNama = $"{nameof(Item)}.{simpleNama}";
                if (columnName == simpleNama || columnName == dottedNama)
                {
                    return string.IsNullOrWhiteSpace(Item.Nama) ? "Nama wajib diisi." : string.Empty;
                }

                return string.Empty;
            }
        }
    }

}
