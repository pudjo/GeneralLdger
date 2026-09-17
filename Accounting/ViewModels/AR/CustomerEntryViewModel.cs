using Accounting.DTO;
using Accounting.Services.ERP;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.ExtendedProperties;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.AR
{
    public  class CustomerEntryViewModel:BaseViewModel
    {
            
        private readonly ICustomerService _customerService;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler RequestClose;
        public event EventHandler<(bool Success, string Message)> OperationCompleted;

        // Bound properties
        public int Id { get; private set; }
        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set
            {
                if (SetProperty(ref _code, value ?? string.Empty))
                    UpdateCommandStates();
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value ?? string.Empty))
                    UpdateCommandStates();
            }
        }


        private string _address = string.Empty;
        public string Address { get => _address; set { _address = value ?? string.Empty; OnPropertyChanged(nameof(Address)); } }

        private string _phone = string.Empty;
        public string Phone { get => _phone; set { _phone = value ?? string.Empty; OnPropertyChanged(nameof(Phone)); } }

        private string _company = string.Empty;
        public string Company { get => _company; set { _company = value ?? string.Empty; OnPropertyChanged(nameof(Company)); } }

        private string _email = string.Empty;
        public string Email { get => _email; set { _email = value ?? string.Empty; OnPropertyChanged(nameof(Email)); } }

        private string _taxId = string.Empty;
        public string TaxId { get => _taxId; set { _taxId = value ?? string.Empty; OnPropertyChanged(nameof(TaxId)); } }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }

        // Commands (CommunityToolkit)
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand UpdateCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IRelayCommand AddCommand { get; }
        public IRelayCommand CloseCommand { get; }



        




        private void UpdateCommandStates()
        {
            // Ensure this runs on UI thread if needed
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                SaveCommand.NotifyCanExecuteChanged();
                UpdateCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
            });
        }

        private bool CanSave() => Id == 0 && !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Name);
        private bool CanUpdate() => Id > 0 && !string.IsNullOrWhiteSpace(Name);
        private bool CanDelete() => Id > 0;

        public CustomerEntryViewModel(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync, CanUpdate);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanDelete);
            AddCommand = new RelayCommand(OnAdd);
            CloseCommand = new RelayCommand(OnClose);

            PrepareNew();


        }

        // Initialize form with an existing DTO for edit, or empty for new
        public void Load(ContactDTO dto)
        {
            if (dto == null)
            {
                PrepareNew();
                return;
            }

            Id = dto.Id;
            Code = dto.Code;
            Name = dto.Name;
            Address = dto.Address;
            Phone = dto.Phone;
            Company = dto.Company;
            Email = dto.Email;
            TaxId = dto.TaxId;
            IsActive = dto.IsActive;
            RaiseCanExecuteChanged();
        }

        private void PrepareNew()
        {
#if DEBUG
            Id = 0;
            Code = "PI";
            Name = "pudjo";
            Address = "Jl Pahlawan 20 A Jakarta";
            Phone = "081211619471";
            Company = "PT BPM";
            Email = "pudjo@yahoo.com";
            TaxId = "89288299282";
#else
     Id = 0;
            Code = string.Empty;
            Name = string.Empty;
            Address = string.Empty;
            Phone = string.Empty;
            Company = string.Empty;
            Email = string.Empty;
            TaxId = string.Empty;
#endif 
            IsActive = true;
            RaiseCanExecuteChanged();
        }


        private void RaiseCanExecuteChanged()
        {
            (SaveCommand as IAsyncRelayCommand)?.NotifyCanExecuteChanged();
            (UpdateCommand as IAsyncRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteCommand as IAsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task SaveAsync()
        {
            try
            {
                var dto = new ContactDTO
                {
                    Code = Code,
                    Name = Name,
                    Address = Address,
                    Phone = Phone,
                    Company = Company,
                    Email = Email,
                    TaxId = TaxId,
                    IsActive = IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = Environment.UserName
                };

                var newId = await _customerService.CreateCustomerAsync(dto, dto.CreatedBy).ConfigureAwait(false);
                if (newId <= 0)
                {
                    OnOperationCompleted(false, "Failed to save customer.");
                    return;
                }

                Id = newId;
                OnOperationCompleted(true, "Customer saved.");
                RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                OnOperationCompleted(false, $"Save failed: {ex.Message}");
            }
        }

        private async Task UpdateAsync()
        {
            try
            {
                if (Id <= 0)
                {
                    OnOperationCompleted(false, "No customer loaded to update.");
                    return;
                }

                var dto = new ContactDTO
                {
                    Id = Id,
                    Code = Code,
                    Name = Name,
                    Address = Address,
                    Phone = Phone,
                    Company = Company,
                    Email = Email,
                    TaxId = TaxId,
                    IsActive = IsActive
                };

                var ok = await _customerService.UpdateCustomerAsync(dto, Environment.UserName).ConfigureAwait(false);
                OnOperationCompleted(ok, ok ? "Customer updated." : "Update failed.");
            }
            catch (Exception ex)
            {
                OnOperationCompleted(false, $"Update failed: {ex.Message}");
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (Id <= 0)
                {
                    OnOperationCompleted(false, "No customer loaded to delete.");
                    return;
                }

                var ok = await _customerService.DeleteCustomerAsync(Id).ConfigureAwait(false);
                if (!ok)
                {
                    OnOperationCompleted(false, "Delete failed.");
                    return;
                }

                OnOperationCompleted(true, "Customer deleted.");
                PrepareNew();
            }
            catch (Exception ex)
            {
                OnOperationCompleted(false, $"Delete failed: {ex.Message}");
            }
        }

        private void OnAdd()
        {
            PrepareNew();
        }

        private void OnClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnOperationCompleted(bool success, string message)
        {
            OperationCompleted?.Invoke(this, (success, message));
            // If success on save/delete/update, notify UI that commands availability may change
            System.Windows.Application.Current?.Dispatcher?.Invoke(RaiseCanExecuteChanged);
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
