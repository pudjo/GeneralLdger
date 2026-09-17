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
    internal class VendorComboViewModel : BaseViewModel
    {
        private IVendorService? _service;

        public ObservableCollection<KeyValuePair<int, string>> Vendors { get; } = new ObservableCollection<KeyValuePair<int, string>>();

        private int? _selectedVendorId;
        public int? SelectedVendorId
        {
            get => _selectedVendorId;
            set
            {
                if (_selectedVendorId == value) return;
                _selectedVendorId = value;
                OnPropertyChanged(nameof(SelectedVendorId));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetService(IVendorService service)
        {
            if (_service != null)
            {
                // unsubscribe previous
                _service.VendorsChanged -= Service_VendorsChanged;
            }

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _service.VendorsChanged += Service_VendorsChanged;

            _ = LoadAsync();
        }
        private void Service_VendorsChanged(object? sender, EventArgs e)
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
                var list = await _service.GetAllVendorsAsync().ConfigureAwait(false);
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Vendors.Clear();
                        foreach (var c in list)
                            Vendors.Add(new KeyValuePair<int, string>(c.Id, c.Name));
                    });
                }
                else
                {
                    Vendors.Clear();
                    foreach (var c in list)
                        Vendors.Add(new KeyValuePair<int, string>(c.Id, c.Name));
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