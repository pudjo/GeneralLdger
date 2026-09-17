using Accounting.Domain.Entities;
using Accounting.DTO.AR;
using Accounting.Services.AR;
using Accounting.Services.ERP;
using Accounting.Services.Inventory;
using Accounting.ViewModels.Controls;
using Accounting.Views.AR;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.AR
{
    internal class ListPenjualanViewModel: BaseViewModel
    {
        private readonly Func<PenjualanDetailView> _entryViewFactory; // factory to create the detail view
        private readonly IPenjualanService _penjualanService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly Func<IDbConnection> _connectionFactory;

        //public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<PenjualanDTO> Results { get; } = new ObservableCollection<PenjualanDTO>();
      //  public ObservableCollection<KeyValuePair<int, string>> Products { get; } = new ObservableCollection<KeyValuePair<int, string>>();

        // Search fields
        public string NoPenjualan { get; set; } = string.Empty;
        public DateRangePickerViewModel DateRange { get; } = new DateRangePickerViewModel();

        

        private int? _selectedProductId;
        public int? SelectedProductId
        {
            get => _selectedProductId;
            set
            {
                if (_selectedProductId == value) return;
                _selectedProductId = value;
                OnPropertyChanged(nameof(SelectedProductId));
            }
        }

        private int? _selectedCustomerId;
        public int? SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                if (_selectedCustomerId == value) return;
                _selectedCustomerId = value;
                OnPropertyChanged(nameof(SelectedCustomerId));
            }
        }

        public string Keterangan { get; set; } = string.Empty;

       

        private PenjualanDTO? _selectedPenjualan;
        public PenjualanDTO? SelectedPenjualan
        {
            get => _selectedPenjualan;
            set
            {
                _selectedPenjualan = value;
                OnPropertyChanged(nameof(SelectedPenjualan));
            }
        }

        public ICustomerService CustomerService { get; }
        public IProductService ProductService { get; }

        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand ShowDetailCommand { get; }

        public event EventHandler<PenjualanDTO>? ShowDetailRequested;

        public ListPenjualanViewModel(
            IPenjualanService penjualanService,
                              ICustomerService customerService,
                              IProductService productService,
            Func<PenjualanDetailView> entryViewFactory
                              )
                              //Func<IDbConnection> connectionFactory)
        {
            _penjualanService = penjualanService ?? throw new ArgumentNullException(nameof(penjualanService));
          
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            CustomerService = _customerService; //

            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            ProductService = _productService; //

            _entryViewFactory = entryViewFactory ?? throw new ArgumentNullException(nameof(entryViewFactory));
            SearchCommand = new DelegateCommand(async _ => await SearchAsync());
            ClearCommand = new DelegateCommand(_ => ClearFilters());
            AddCommand = new RelayCommand(() => _ = OpenEntryAsync(null));
            ShowDetailCommand = new RelayCommand<object?>(p => _ = OpenEntryAsync(p as PenjualanDTO));


            _ = SearchAsync();
        }

        private void ClearFilters()
        {
            NoPenjualan = string.Empty;
            DateRange.Reset();
            SelectedCustomerId = null;
            Keterangan = string.Empty;
            SelectedProductId = null;

            OnPropertyChanged(nameof(NoPenjualan));
            OnPropertyChanged(nameof(DateRange));
            OnPropertyChanged(nameof(SelectedCustomerId));
            OnPropertyChanged(nameof(Keterangan));
            OnPropertyChanged(nameof(SelectedProductId));
        }

        private async Task SearchAsync()
        {
            try
            {
                var items = await _penjualanService.GetAllAsync().ConfigureAwait(false);
                var query = items.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(NoPenjualan))
                {
                    query = query.Where(p => (p.Description ?? string.Empty).IndexOf(NoPenjualan, StringComparison.InvariantCultureIgnoreCase) >= 0
                                              || p.Id.ToString().IndexOf(NoPenjualan, StringComparison.InvariantCultureIgnoreCase) >= 0);
                }

                if (DateRange.IsDailyMode && DateRange.FirstDate != null )
                {
                    query = query.Where(p => p.Tanggal >= DateRange.FirstDate);
                }

                if (DateRange.IsDailyMode && DateRange.EndDate !=null )
                {
                    var end = DateRange.EndDate.AddDays(1).AddTicks(-1);
                    query = query.Where(p => p.Tanggal <= end);
                }

                if (DateRange.IsMonthlyMode && DateRange.SelectedMonthIndex >= 0)
                {
                    var month = DateRange.SelectedMonthIndex + 1;
                    var year = DateTime.Now.Year;
                    var first = new DateTime(year, month, 1);
                    var last = first.AddMonths(1).AddTicks(-1);
                    query = query.Where(p => p.Tanggal >= first && p.Tanggal <= last);
                }

                if (SelectedCustomerId.HasValue)
                {
                    query = query.Where(p => p.CutsID == SelectedCustomerId.Value);
                }

                if (!string.IsNullOrWhiteSpace(Keterangan))
                {
                    query = query.Where(p => (p.Description ?? string.Empty).IndexOf(Keterangan, StringComparison.InvariantCultureIgnoreCase) >= 0);
                }

                if (SelectedProductId.HasValue)
                {
                    query = query.Where(p => (p.Details ?? new List<PenjualanDetailDTO>()).Any(d => d.ProductId == SelectedProductId.Value));
                }

                var list = query.OrderByDescending(p => p.Tanggal).ToList();

                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Results.Clear();
                        foreach (var r in list) Results.Add(r);
                    });
                }
                else
                {
                    Results.Clear();
                    foreach (var r in list) Results.Add(r);
                }
            }
            catch
            {
                // handle/log as needed
            }
        }

        // Replace original OnAdd / OnShowDetail implementations with this async opener
        private async Task OpenEntryAsync(PenjualanDTO? dto)
        {
            // create a fresh view (DI factory should return new instance)
            var entryView = _entryViewFactory();
            if (entryView == null) return;

            // Expect the view to have its VM injected as DataContext (transient)
            if (entryView.DataContext is not PenjualanDetailViewModel entryVm)
            {
                // defensive fallback: try to get VM from view service provider, otherwise abort
                return;
            }

            // Load DTO (null => new)
            entryVm.Load(dto?.Id);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnOperationCompleted(object? s, (bool Success, string Message) result)
            {
                entryVm.OperationCompleted -= OnOperationCompleted;
                entryVm.RequestClose -= OnRequestClose;
                try
                {
                    if (entryView is Window w && w.IsVisible) w.Close();
                }
                catch { }
                tcs.TrySetResult(result.Success);
            }

            void OnRequestClose(object? s, EventArgs e)
            {
                entryVm.OperationCompleted -= OnOperationCompleted;
                entryVm.RequestClose -= OnRequestClose;
                try
                {
                    if (entryView is Window w && w.IsVisible) w.Close();
                }
                catch { }
                tcs.TrySetResult(false);
            }

            entryVm.OperationCompleted += OnOperationCompleted;
            entryVm.RequestClose += OnRequestClose;

            // Show the view:
            if (entryView is Window win)
            {
                // window provided by DI -> show as dialog
                win.Owner = Application.Current?.MainWindow;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                win.SizeToContent = SizeToContent.WidthAndHeight;
                win.ResizeMode = ResizeMode.NoResize;

                // ensure window closes when VM requests it
                void CloseOnRequest(object? s, EventArgs e) => win.Close();
                entryVm.RequestClose += CloseOnRequest;

                win.ShowDialog();

                entryVm.RequestClose -= CloseOnRequest;
            }
            else
            {
                // host user control inside a transient window
                var host = new Window
                {
                    Title = dto == null || dto.Id == 0 ? "Add Penjualan" : "Penjualan Detail",
                    Content = entryView,
                    Owner = Application.Current?.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize
                };

                void CloseHostOnRequest(object? s, EventArgs e) => host.Close();
                entryVm.RequestClose += CloseHostOnRequest;

                host.ShowDialog();

                entryVm.RequestClose -= CloseHostOnRequest;
            }

            var success = await tcs.Task.ConfigureAwait(true);

            // Refresh list on success
            if (success)
            {
                if (Application.Current?.Dispatcher != null)
                    Application.Current.Dispatcher.InvokeAsync(() => SearchCommand.Execute(null));
                else
                    await SearchAsync().ConfigureAwait(false);
            }

            // cleanup subscriptions
            entryVm.OperationCompleted -= OnOperationCompleted;
            entryVm.RequestClose -= OnRequestClose;
        }


        //protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    internal static class ListExtensions
    {
        public static void AddRange<T>(this List<T> list, IEnumerable<T> items)
        {
            foreach (var i in items) list.Add(i);
        }
    }
}

