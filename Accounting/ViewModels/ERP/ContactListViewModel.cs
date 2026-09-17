using Accounting.Domain.Enum;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Services.ERP;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.ERP
{
    
        internal class ContactListViewModel : BaseViewModel
        {
            private readonly IContactQueryRepository _queryRepo;
            private readonly Func<ContactType, IContactService> _serviceFactory;
            private readonly ILogger<ContactListViewModel> _logger;
            private readonly IValidator<ContactDTO>? _validator;
            private readonly ContactType _contactType;

            private ObservableCollection<ContactDTO> _contacts = new();
            public ObservableCollection<ContactDTO> Contacts
            {
                get => _contacts;
                set => SetProperty(ref _contacts, value);
            }

            private ContactDTO _selectedContact;
            public ContactDTO SelectedContact
            {
                get => _selectedContact;
                set => SetProperty(ref _selectedContact, value);
            }

            private string _searchText;
            public string SearchText
            {
                get => _searchText;
                set => SetProperty(ref _searchText, value);
            }

            public IRelayCommand AddCommand { get; }
            public IRelayCommand SearchCommand { get; }
            public IRelayCommand<ContactDTO> OpenDetailCommand { get; }
            public IRelayCommand RefreshCommand { get; }

            public ContactListViewModel(
                IContactQueryRepository queryRepo,
                Func<ContactType, IContactService> serviceFactory,
                ILogger<ContactListViewModel> logger,
                IValidator<ContactDTO>? validator = null,
                ContactType contactType = ContactType.Customer)
            {
                _queryRepo = queryRepo ?? throw new ArgumentNullException(nameof(queryRepo));
                _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _validator = validator;
                _contactType = contactType;

                AddCommand = new RelayCommand(OnAdd);
                SearchCommand = new RelayCommand(async () => await OnSearchAsync());
                OpenDetailCommand = new RelayCommand<ContactDTO>(OnOpenDetail);
                RefreshCommand = new RelayCommand(async () => await LoadAsync());

                // initial load
                _ = LoadAsync();
            }

            // <-- This method must exist inside the class. Keep it non-static and correctly spelled.
            public async Task LoadAsync()
            {
                try
                {
                    var all = await _queryRepo.GetAllAsync().ConfigureAwait(false);
                    var filtered = all?
                        .Where(c => c.Type == _contactType)
                        .OrderBy(c => c.Name)
                        .ToList()
                        ?? new System.Collections.Generic.List<ContactDTO>();

                    // update on UI thread
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        Contacts = new ObservableCollection<ContactDTO>(filtered);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Load contacts failed");
                }
            }

            private async Task OnSearchAsync()
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(SearchText))
                    {
                        await LoadAsync();
                        return;
                    }

                    var results = await _queryRepo.SearchAsync(SearchText).ConfigureAwait(false);
                    var filtered = results?
                        .Where(c => c.Type == _contactType)
                        .OrderBy(c => c.Name)
                        .ToList()
                        ?? new System.Collections.Generic.List<ContactDTO>();

                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        Contacts = new ObservableCollection<ContactDTO>(filtered);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Search contacts failed");
                }
            }

            private void OnAdd()
            {
                var dto = new ContactDTO { Type = _contactType };
                OpenDetailWindow(dto);
            }

            private void OnOpenDetail(ContactDTO dto)
            {
                if (dto == null) return;
                OpenDetailWindow(dto);
            }

            private void OpenDetailWindow(ContactDTO dto)
            {
                try
                {
                    var contactService = _serviceFactory(_contactType);
                    var vm = new ContactDetailViewModel(contactService, _validator, dto);
                    var win = new Accounting.Views.ERP.ContactDetailWindow
                    {
                        DataContext = vm,
                        Owner = Application.Current?.MainWindow
                    };

                    vm.RequestClose += async (s, saved) =>
                    {
                        win.DialogResult = saved;
                        win.Close();
                        if (saved) await LoadAsync();
                    };

                    win.ShowDialog();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OpenDetailWindow failed");
                }
            }
        }
    }
    
