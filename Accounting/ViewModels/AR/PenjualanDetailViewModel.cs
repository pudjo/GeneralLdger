using Accounting.DTO.AR;
using Accounting.Services.AR;
using Accounting.Services.ERP;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.AR
{
    public class PenjualanDetailViewModel : BaseViewModel
    {
        private readonly IPenjualanService _penjualanService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<PenjualanDetailViewModel> _logger;

        public event EventHandler? RequestClose;
        public event EventHandler<(bool Success, string Message)> OperationCompleted;

        // Header
        private string _noDokumen = string.Empty;
        public string NoDokumen { get => _noDokumen; set => SetProperty(ref _noDokumen, value); }

        private DateTime _tanggal = DateTime.Now;
        public DateTime Tanggal { get => _tanggal; set => SetProperty(ref _tanggal, value); }

        private int? _selectedCustomerId;
        public int? SelectedCustomerId { get => _selectedCustomerId; set => SetProperty(ref _selectedCustomerId, value); }

        private string _keterangan = string.Empty;
        public string Keterangan { get => _keterangan; set => SetProperty(ref _keterangan, value); }

        // Lookup services exposed so XAML can bind ProductService/CustomerService via RelativeSource
        public IProductService ProductService => _productService;
        public ICustomerService CustomerService => _customerService;

        // Details
        
        // Commands
        public IRelayCommand AddDetailLineCommand { get; }
        public IRelayCommand RemoveDetailCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand UpdateCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IRelayCommand CloseCommand { get; }
        public ObservableCollection<PenjualanDetailDTO> Details { get; } = new ObservableCollection<PenjualanDetailDTO>();

        public decimal DetailsTotal => Details.Sum(d => d.Total);

        public decimal DetailsQuantityTotal => Details.Sum(d => d.Quantity);

        public PenjualanDetailViewModel(IPenjualanService penjualanService,
                                 IProductService productService,
                                 ICustomerService customerService,
                                 ILogger<PenjualanDetailViewModel> logger)
        {

            _penjualanService = penjualanService ?? throw new ArgumentNullException(nameof(penjualanService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AddDetailLineCommand = new RelayCommand(OnAddDetailLine);
            RemoveDetailCommand = new RelayCommand<object?>(p => OnRemoveDetail(p as PenjualanDetailDTO));
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync, CanUpdate);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanDelete);
            CloseCommand = new RelayCommand(OnClose);

            Details.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (PenjualanDetailDTO d in e.NewItems)
                        d.PropertyChanged += Detail_PropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (PenjualanDetailDTO d in e.OldItems)
                        d.PropertyChanged -= Detail_PropertyChanged;
                }
                OnPropertyChanged(nameof(DetailsTotal));
                OnPropertyChanged(nameof(DetailsQuantityTotal));
            };

            PrepareNew();
        }

        public void Load(int? penjualanId = null)
        {
            if (!penjualanId.HasValue)
            {
                PrepareNew();
                return;
            }

            // load existing penjualan via _penjualanService if needed
        }

        private void PrepareNew()
        {
            NoDokumen = string.Empty;
            Tanggal = DateTime.Now;
            SelectedCustomerId = null;
            Keterangan = string.Empty;
            Details.Clear();
            Details.Add(new PenjualanDetailDTO());
            OnPropertyChanged(nameof(DetailsTotal));
            NotifyCommandStates();
        }

        private void OnAddDetailLine()
        {
            Details.Add(new PenjualanDetailDTO());
            OnPropertyChanged(nameof(DetailsTotal));
            NotifyCommandStates();
        }

        private void OnRemoveDetail(PenjualanDetailDTO? dto)
        {
            if (dto == null) return;
            Details.Remove(dto);
            OnPropertyChanged(nameof(DetailsTotal));
            NotifyCommandStates();
        }
        private void Detail_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is PenjualanDetailDTO dto)
            {
                if (e.PropertyName == nameof(PenjualanDetailDTO.ProductId))
                {
                    _ = SyncProductToDetailAsync(dto);
                }
                else if (e.PropertyName == nameof(PenjualanDetailDTO.Quantity) ||
                         e.PropertyName == nameof(PenjualanDetailDTO.Price) ||
                         e.PropertyName == nameof(PenjualanDetailDTO.SellingPrice))
                {
                    OnPropertyChanged(nameof(DetailsTotal));
                    OnPropertyChanged(nameof(DetailsQuantityTotal));
                }
            }
        }
        private async Task SyncProductToDetailAsync(PenjualanDetailDTO dto)
        {
            try
            {
                if (dto == null) return;
                if (dto.ProductId <= 0) return;

                var p = await _productService.GetByIdAsync(dto.ProductId).ConfigureAwait(false);
                if (p != null)
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        dto.ProductName = p.Name ?? string.Empty;
                        dto.SellingPrice = p.CurrentSellingPrice;
                        OnPropertyChanged(nameof(DetailsTotal));
                        OnPropertyChanged(nameof(DetailsQuantityTotal));
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncProductToDetailAsync failed");
            }
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(NoDokumen) &&
            SelectedCustomerId.HasValue &&
            Details.Any(d => d.ProductId > 0 && d.Quantity > 0);
        private bool CanUpdate() => false;
        private bool CanDelete() => false;

        private void NotifyCommandStates()
        {
            (SaveCommand as IAsyncRelayCommand)?.NotifyCanExecuteChanged();
            (UpdateCommand as IAsyncRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteCommand as IAsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task SaveAsync()
        {
            try
            {
                var master = new Accounting.DTO.AR.PenjualanDTO
                {
                    Description = NoDokumen,
                    Tanggal = Tanggal,
                    CutsID = SelectedCustomerId ?? 0,
                    AddressLine1 = Keterangan,
                    Details = Details.ToList()
                };

                var newId = await _penjualanService.CreateAsync(master, Details.ToList()).ConfigureAwait(false);
                if (newId > 0)
                {
                    OperationCompleted?.Invoke(this, (true, "Penjualan disimpan."));
                    RequestClose?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    OperationCompleted?.Invoke(this, (false, "Gagal menyimpan penjualan."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveAsync failed");
                OperationCompleted?.Invoke(this, (false, ex.Message));
            }
        }

        private async Task UpdateAsync()
        {
            await Task.CompletedTask;
        }

        private async Task DeleteAsync()
        {
            await Task.CompletedTask;
        }

        private void OnClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}