using Accounting.Services.ERP;
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
    internal class CustomerComboViewModel:BaseViewModel
    {
        private ICustomerService? _service;

        public ObservableCollection<KeyValuePair<int, string>> Customers { get; } = new ObservableCollection<KeyValuePair<int, string>>();

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetService(ICustomerService service)
        {
            if (_service != null)
            {
                // unsubscribe previous
                _service.CustomersChanged -= Service_CustomersChanged;
            }

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _service.CustomersChanged += Service_CustomersChanged;

            _ = LoadAsync();
        }
        private void Service_CustomersChanged(object? sender, EventArgs e)
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

        private async Task LoadAsync()
        {
            try
            {
                if (_service == null) return;
                var list = await _service.GetAllCustomersAsync().ConfigureAwait(false);
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Customers.Clear();
                        foreach (var c in list)
                            Customers.Add(new KeyValuePair<int, string>(c.Id, c.Name));
                    });
                }
                else
                {
                    Customers.Clear();
                    foreach (var c in list)
                        Customers.Add(new KeyValuePair<int, string>(c.Id, c.Name));
                }
            }
            catch
            {
                // swallow; in production add logging
            }
        }

        //protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
