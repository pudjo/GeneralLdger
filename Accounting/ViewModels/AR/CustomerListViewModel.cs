using Accounting.DTO;
using Accounting.Services.ERP;
using Accounting.Views.AR;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Accounting.ViewModels.AR
{

    internal class CustomerListViewModel : BaseViewModel
    {
        private readonly ICustomerService _customerService;
        private readonly Func<CustomerEntryView> _entryViewFactory;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ContactDTO> Results { get; } = new ObservableCollection<ContactDTO>();

        private ContactDTO? _selectedCustomer;
        public ContactDTO? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer == value) return;
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
            }
        }

        private string _codeFilter = string.Empty;
        public string CodeFilter
        {
            get => _codeFilter;
            set
            {
                if (_codeFilter == value) return;
                _codeFilter = value ?? string.Empty;
                OnPropertyChanged(nameof(CodeFilter));
            }
        }

        private string _nameFilter = string.Empty;
        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                if (_nameFilter == value) return;
                _nameFilter = value ?? string.Empty;
                OnPropertyChanged(nameof(NameFilter));
            }
        }

        public IAsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand ShowDetailCommand { get; }

        public CustomerListViewModel(ICustomerService customerService, Func<CustomerEntryView> entryViewFactory)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _entryViewFactory = entryViewFactory ?? throw new ArgumentNullException(nameof(entryViewFactory));

            SearchCommand = new AsyncRelayCommand(SearchAsync);
            AddCommand = new AsyncRelayCommand(async () => await OpenEntryAsync(null));
            ShowDetailCommand = new AsyncRelayCommand<ContactDTO?>(async dto => await OpenEntryAsync(dto));

            _ = SearchAsync();
        }

        private async Task OpenEntryAsync(ContactDTO? dto)
        {
            
                // 1. Buat view dari factory
                var entryView = _entryViewFactory();
                if (entryView == null) return;

                // 2. Cek DataContext
                if (entryView.DataContext is not CustomerEntryViewModel entryVm) return;

                // 3. Load DTO
                entryVm.Load(dto);

                // 4. Buat Host Window untuk membungkus UserControl
                var host = new Window
                {
                    Title = dto == null || dto.Id == 0 ? "Add Customer" : "Customer Detail",
                    Content = entryView,
                    Owner = Application.Current?.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize
                };

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                // Helper untuk menutup host window secara aman di UI Thread
                void SafeCloseHost()
                {
                    void CloseAction()
                    {
                        if (host.IsVisible)
                        {
                            host.Close();
                        }
                    }

                    if (host.Dispatcher.CheckAccess())
                        CloseAction();
                    else
                        host.Dispatcher.Invoke(CloseAction);
                }

                void OnOpCompleted(object? s, (bool Success, string Message) result)
                {
                    entryVm.OperationCompleted -= OnOpCompleted;
                    entryVm.RequestClose -= OnRequestClose;

                    SafeCloseHost();
                    tcs.TrySetResult(result.Success);
                }

                void OnRequestClose(object? s, EventArgs e)
                {
                    entryVm.OperationCompleted -= OnOpCompleted;
                    entryVm.RequestClose -= OnRequestClose;

                    SafeCloseHost();
                    tcs.TrySetResult(false);
                }

                // Pasang Event Handler
                entryVm.OperationCompleted += OnOpCompleted;
                entryVm.RequestClose += OnRequestClose;

                // Menangani jika user menutup window via tombol 'X' di kanan atas window
                void OnWindowClosed(object? sender, EventArgs e)
                {
                    host.Closed -= OnWindowClosed;
                    // Jika tcs belum selesai (user klik 'X' bukan tombol Save/Cancel)
                    tcs.TrySetResult(false);
                }
                host.Closed += OnWindowClosed;

                // 5. Tampilkan Dialog
                if (Application.Current?.Dispatcher != null)
                    Application.Current.Dispatcher.Invoke(() => host.ShowDialog());
                else
                    host.ShowDialog();

                // 6. Tunggu dialog selesai/ditutup
                var success = await tcs.Task.ConfigureAwait(false);

                // 7. Refresh list jika berhasil
                if (success)
                {
                    if (Application.Current?.Dispatcher != null)
                        Application.Current.Dispatcher.Invoke(() => SearchCommand.Execute(null));
                    else
                        await SearchAsync().ConfigureAwait(false);
                }

                // 8. Cleanup
                entryVm.OperationCompleted -= OnOpCompleted;
                entryVm.RequestClose -= OnRequestClose;
                host.Closed -= OnWindowClosed;
            }

        private async Task SearchAsync()
        {
            try
            {
                ContactDTO[] found = Array.Empty<ContactDTO>();

                if (!string.IsNullOrWhiteSpace(CodeFilter))
                {
                    var byCode = await _customerService.GetCustomerByCodeAsync(CodeFilter).ConfigureAwait(false);
                    if (byCode != null) found = new[] { byCode };
                }
                else if (!string.IsNullOrWhiteSpace(NameFilter))
                {
                    var list = await _customerService.SearchCustomersAsync(NameFilter).ConfigureAwait(false);
                    found = list?.ToArray() ?? Array.Empty<ContactDTO>();
                }
                else
                {
                    var all = await _customerService.GetAllCustomersAsync().ConfigureAwait(false);
                    found = all?.ToArray() ?? Array.Empty<ContactDTO>();
                }

                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Results.Clear();
                        foreach (var c in found) Results.Add(c);
                    });
                }
                else
                {
                    Results.Clear();
                    foreach (var c in found) Results.Add(c);
                }
            }
            catch
            {
                // log as needed
            }
        }

      //  protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}


