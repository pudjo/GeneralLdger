using Accounting.DTO;
using Accounting.Services.Accounts;

using Accounting.Services.GeneralLedgerService;
using Accounting.ViewModels.Controls;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.GeneralLedger
{
    internal class GeneralLedgerViewModel : BaseViewModel
    {
        private AccountService accountService;
        private GeneralLedgerService generalLedgerService;
        private ObservableCollection<AccountDTO> _akunTreeList;
        private string _kataKunciPencarian;
        private ObservableCollection<GeneralLedgerDTO> _generalLedgerRows;
        private ObservableCollection<GeneralLedgerDTO> _generalLedgerRowsDariDatabase = new ObservableCollection<GeneralLedgerDTO>();

        // TAMBAHAN: DateRangePickerViewModel property
        private DateRangePickerViewModel _dateRangePicker;
        
        private string _searchText;
        private List<AccountDTO> _originalList; // Menyimpan data asli dari database

        // Properti untuk Binding ke TextBox
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }
        public ObservableCollection<GeneralLedgerDTO> GeneralLedgerRows
        {
            get => _generalLedgerRows;
            set
            {
                _generalLedgerRows = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property untuk DateRangePickerControl
        /// </summary>
        public DateRangePickerViewModel DateRangePicker
        {
            get => _dateRangePicker;
            set
            {
                _dateRangePicker = value;
                OnPropertyChanged();
            }
        }

        public string KataKunciPencarian
        {
            get => _kataKunciPencarian;
            set
            {
                if (SetProperty(ref _kataKunciPencarian, value))
                {
                    EksekusiCari();
                }
            }
        }
        public ICommand SearchCommand => new RelayCommand(() => ExecuteSearch());
        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ResetHighlight(_originalList);
                return;
            }

            string term = SearchText.ToLower();
            ApplyHighlight(_originalList, term);

        }
        private void ApplyHighlight(IEnumerable<AccountDTO> items, string term)
        {
            foreach (var item in items)
            {
                // Cek apakah item ini cocok
                item.IsMatch = item.Name.ToLower().Contains(term) || item.Id.ToLower().Contains(term);

                // Jika cocok, kita expand parent-nya agar terlihat
                if (item.IsMatch)
                {
                    ExpandParents(item);
                }

                if (item.Children != null && item.Children.Any())
                {
                    ApplyHighlight(item.Children, term);
                }
            }
        }

        private void ResetHighlight(IEnumerable<AccountDTO> items)
        {
            foreach (var item in items)
            {
                item.IsMatch = false;
                if (item.Children != null) ResetHighlight(item.Children);
            }
        }
        private void ExpandParents(AccountDTO item)
        {
            if (item == null) return;

            // Kita cari siapa orang tua dari item ini di dalam master data (_originalList)
            var parent = FindParent(_originalList, item.Id);

            if (parent != null)
            {
                parent.IsExpanded = true;
                // Rekursif ke atas sampai ketemu Root
                ExpandParents(parent);
            }
        }

        private AccountDTO FindParent(IEnumerable<AccountDTO> items, string childId)
        {
            foreach (var item in items)
            {
                if (item.Children != null && item.Children.Any(c => c.Id == childId))
                {
                    return item;
                }

                if (item.Children != null)
                {
                    var found = FindParent(item.Children, childId);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void EksekusiCari()
        {
            if (string.IsNullOrWhiteSpace(KataKunciPencarian))
            {
                // Jika kosong, kembalikan ke data asli
                GeneralLedgerRows = new ObservableCollection<GeneralLedgerDTO>(_generalLedgerRowsDariDatabase);
            }
            else
            {
                var kataKunci = KataKunciPencarian.ToLower();
                var hasil = _generalLedgerRowsDariDatabase.Where(x =>
                    (x.AccountCode?.ToLower().Contains(kataKunci) ?? false) ||
                    (x.AccountName?.ToLower().Contains(kataKunci) ?? false) ||
                    (x.RefNo?.ToLower().Contains(kataKunci) ?? false) ||
                    (x.Description?.ToLower().Contains(kataKunci) ?? false)
                ).ToList();

                GeneralLedgerRows = new ObservableCollection<GeneralLedgerDTO>(hasil);
            }
        }

        private void EksekusiReset()
        {
            KataKunciPencarian = string.Empty;
            GeneralLedgerRows = new ObservableCollection<GeneralLedgerDTO>(_generalLedgerRowsDariDatabase);
        }

        private decimal _totalDebet;
        public decimal TotalDebet
        {
            get => _totalDebet;
            set
            {
                _totalDebet = value;
                OnPropertyChanged();
            }
        }
        private decimal _saldoAwal;
        public decimal SaldoAwal
        {
            get => _saldoAwal;
            set
            {
                _saldoAwal = value;
                OnPropertyChanged();
            }
        }
        private decimal _totalCredit;
        public decimal TotalCredit
        {
            get => _totalCredit;
            set
            {
                _totalCredit = value;
                OnPropertyChanged();
            }
        }


        private AccountDTO _selectedAkun;
        public AccountDTO SelectedAkun
        {
            get => _selectedAkun;
            set
            {
                _selectedAkun = value;
                OnPropertyChanged();
            }
        }

        bool _isExpanded;
        bool _isSelected;

        #region IsExpanded
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region IsSelected
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public ObservableCollection<AccountDTO> AkunTreeList
        {
            get => _akunTreeList;
            set
            {
                _akunTreeList = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadGeneralLedgerCommand { get; }
        
        public ICommand ResetSearchCommand { get; }

        public GeneralLedgerViewModel(
            AccountService _accountService,
            GeneralLedgerService _generalLedgerService)
        {
            accountService = _accountService;
            generalLedgerService = _generalLedgerService;

            // TAMBAHAN: Inisialisasi DateRangePickerViewModel
            DateRangePicker = new DateRangePickerViewModel();

            LoadGeneralLedgerCommand = new RelayCommand(() => LoadGeneralLedger());
           
            ResetSearchCommand = new RelayCommand(EksekusiReset);
            LoadTreeData();

            // Load data saat view pertama kali dibuka
          //  LoadGeneralLedger();
        }
        private async void LoadTreeData()
        {

            try
            {
                List<AccountDTO> rawData = await accountService.GetTreeAccounts();
                AkunTreeList = new ObservableCollection<AccountDTO>(rawData);

                _originalList = rawData; // Simpan master data
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail get data: {ex.Message}", "Error Sistem",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        
        /// <summary>
        /// Load General Ledger data berdasarkan DateRangePicker yang dipilih
        /// </summary>
        private async void LoadGeneralLedger()
        {
            try
            {
                // Gunakan DateRangePickerViewModel untuk mendapatkan range tanggal
                DateTime startDate = DateRangePicker.GetFirstDate();
                DateTime endDate = DateRangePicker.GetEndDate();

                // Load data dari service
                //var data = await generalLedgerService.GetGeneralLedgers(startDate, endDate);
                // hitung saldo sebelum tanggal awal
                string accountCode = string.Empty;
                if (SelectedAkun !=null )
                {
                    
                    accountCode = SelectedAkun.Id;

                }

                var data = await generalLedgerService.GetGeneralLedgers(startDate, endDate, accountCode);
                
                // hilangkan saldo Awalnya
                
               ObservableCollection<GeneralLedgerDTO> _generalLedgerRowsDariDatabaseAdasadoAwal = new ObservableCollection<GeneralLedgerDTO>();

                _generalLedgerRowsDariDatabaseAdasadoAwal=new ObservableCollection<GeneralLedgerDTO>(data);


                decimal saldoAwal = _generalLedgerRowsDariDatabaseAdasadoAwal.Where(id=>id.JournalHeaderId==0).Sum(x => x.Debet - x.Credit);
                SaldoAwal = saldoAwal;


                _generalLedgerRowsDariDatabase = new ObservableCollection<GeneralLedgerDTO>((data).Where(id => id.JournalHeaderId > 0));
                GeneralLedgerRows = new ObservableCollection<GeneralLedgerDTO>(_generalLedgerRowsDariDatabase);
                TotalDebet= GeneralLedgerRows.Sum(x => x.Debet);
                TotalCredit= GeneralLedgerRows.Sum(x => x.Credit);





                CalculateRunningBalance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading General Ledger: {ex.Message}");
            }
        }
        public void CalculateRunningBalance()
        {
            decimal runningBalance = 0;
            if (GeneralLedgerRows == null)
            {
                return;
            }
            // Anggaplah 'GeneralLedgerRows' adalah ObservableCollection yang di-binding ke XAML
            foreach (var row in GeneralLedgerRows)
            {
                // Rumus Saldo Berjalan (Asumsi Akun Normal Debet seperti Kas/Bank)
                runningBalance = runningBalance + row.Debet - row.Credit;

                // Masukkan hasil hitungan ke kolom Saldo baris tersebut
                row.Saldo = runningBalance;
            }
        }
    }
}
