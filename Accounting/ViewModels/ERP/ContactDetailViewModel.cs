using Accounting.Domain.Entities;
using Accounting.Domain.Enum;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Services.ERP;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.ERP
{

     internal class ContactDetailViewModel : BaseViewModel, IDataErrorInfo
        {
            private readonly IContactService _contactService;
            private readonly IValidator<ContactDTO>? _validator;
            private ValidationResult? _lastValidationResult;

            public ContactDTO Contact { get; set; }

            public IEnumerable<Domain.Enum.ContactType> ContactTypes => (IEnumerable<Domain.Enum.ContactType>)Enum.GetValues(typeof(Domain.Enum.ContactType));

            public IRelayCommand SaveCommand { get; }
            public IRelayCommand CancelCommand { get; }

            public event EventHandler<bool> RequestClose; // bool = saved

            public ContactDetailViewModel(IContactService contactService, IValidator<ContactDTO>? validator, ContactDTO contact)
            {
                _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
                _validator = validator;
                Contact = contact ?? new ContactDTO();

                SaveCommand = new RelayCommand(async () => await OnSaveAsync());
                CancelCommand = new RelayCommand(OnCancel);
            }

            private void EnsureValidation()
            {
                if (_validator != null)
                {
                    _lastValidationResult = _validator.Validate(Contact);
                }
                else
                {
                    var failures = new FluentValidation.Results.ValidationResult();
                    if (string.IsNullOrWhiteSpace(Contact.Name))
                        failures.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(Contact.Name), "Nama wajib diisi."));
                    if (string.IsNullOrWhiteSpace(Contact.Code))
                        failures.Errors.Add(new FluentValidation.Results.ValidationFailure(nameof(Contact.Code), "Kode wajib diisi."));
                    _lastValidationResult = failures;
                }
            }

            private async Task OnSaveAsync()
            {
                try
                {
                    EnsureValidation();
                    if (_lastValidationResult != null && !_lastValidationResult.IsValid)
                    {
                        var sb = new StringBuilder();
                        foreach (var err in _lastValidationResult.Errors)
                            sb.AppendLine(err.ErrorMessage);

                        MessageBox.Show(sb.ToString(), "Validasi gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (Contact.Id == 0)
                    {
                        var newId = await _contactService.CreateAsync(Contact, Contact.CreatedBy ?? "system").ConfigureAwait(false);
                        if (newId > 0)
                        {
                            Contact.Id = newId;
                            RequestClose?.Invoke(this, true);
                        }
                        else
                        {
                            MessageBox.Show("Gagal menyimpan contact.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        var ok = await _contactService.UpdateAsync(Contact, Contact.CreatedBy ?? "system").ConfigureAwait(false);
                        if (ok)
                        {
                            RequestClose?.Invoke(this, true);
                        }
                        else
                        {
                            MessageBox.Show("Gagal mengupdate contact.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void OnCancel() => RequestClose?.Invoke(this, false);

            // IDataErrorInfo implementation (same as previous)
            public string Error { get { EnsureValidation(); return _lastValidationResult == null ? string.Empty : string.Join(Environment.NewLine, _lastValidationResult.Errors.Select(e => e.ErrorMessage)); } }
            public string this[string columnName] { get { EnsureValidation(); return _lastValidationResult == null ? string.Empty : (_lastValidationResult.Errors.FirstOrDefault(e => e.PropertyName == columnName)?.ErrorMessage ?? string.Empty); } }
        }

    }

