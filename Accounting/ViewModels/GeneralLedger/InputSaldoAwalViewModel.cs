using Accounting.Domain;
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.Services.Accounts;

using Accounting.Services.GeneralLedgerService;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.GeneralLedger
{

    internal class InputSaldoAwalViewModel : BaseViewModel
    {
        private SaldoAwalService saldoAwalService;
        private AccountService accountService;
        private SaldoAwalReadService saldoawalReadService;
        private List<SaldoAwalDTO> _saldoAwal = new List<SaldoAwalDTO>();

        private string _dataGridSearchText;
        public string DataGridSearchText
        {
            get => _dataGridSearchText;
            set
            {
                if (SetProperty(ref _dataGridSearchText, value))
                {

                    SearchingText();
                }
            }
        }
        private void SearchingText()
        {
            if (string.IsNullOrWhiteSpace(DataGridSearchText))
            {
                SaldoAwalRows = saldoAwalBackup;
                // Jika kosong, kembalikan ke data asli
                SaldoAwalRows = new ObservableCollection<SaldoAwalDTO>(saldoAwalBackup);
            }
            else
            {
                var kataKunci = DataGridSearchText.ToLower();
                var hasil = saldoAwalBackup.Where(x =>
                    (x.AccountName?.ToLower().Contains(kataKunci) ?? false) ||
                    (x.AccountCode?.ToLower().Contains(kataKunci) ?? false) 
                    
                ).ToList();

                SaldoAwalRows = new ObservableCollection<SaldoAwalDTO>(hasil);
            }
        }


        private int year;
        public int Year  
        {
            get => year;
            set
            {
                year = value;
                OnPropertyChanged();
            }
        }
        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<AccountDTO> _akunTreeList;
        private ObservableCollection<SaldoAwalDTO> saldoAwalBackup;// untuk pencaraib
        // Data will be displayed in DataGrid
        private ObservableCollection<SaldoAwalDTO> _saldoAwalRows;
        
        public ObservableCollection<SaldoAwalDTO> SaldoAwalRows
        {
            get => _saldoAwalRows;
            set
            {
                _saldoAwalRows = value;
                OnPropertyChanged(nameof(SaldoAwalRows));
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
                    this.OnPropertyChanged("IsExpanded");
                }

            }
        }

        #endregion // IsExpanded

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
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected
        public decimal TotalDebet => SaldoAwalRows?.Sum(x => x.Debet) ?? 0;

        // Properti untuk Total Kredit (Hanya Read-Only, kalkulasi otomatis)
        public decimal TotalCredit => SaldoAwalRows?.Sum(x => x.Credit) ?? 0;
        public ICommand ShowDetailCommand { get; }
        public IRelayCommand CallDataSaldoAwal { get; }
        public IRelayCommand SimpanSaldoAwalCommand  { get; }
        public InputSaldoAwalViewModel(SaldoAwalService _saldoAwalService,
        AccountService _accountService,SaldoAwalReadService _saldoawalReadService)
        {
            accountService = _accountService;// App.ServiceProvider.GetRequiredService<AccountService>();
            saldoAwalService = _saldoAwalService;// App.ServiceProvider.GetRequiredService<GeneralLedgerService>();
            saldoawalReadService= _saldoawalReadService;
            ShowDetailCommand = new RelayCommand<AccountDTO>(OnShowDetail);
            CallDataSaldoAwal = new RelayCommand(CallDataSaldoAwalAction);
            SimpanSaldoAwalCommand = new RelayCommand(SimpanSaldoAwalAction, ()=> IsDataReadyToSave);
            SaldoAwalRows = new ObservableCollection<SaldoAwalDTO>();
            saldoAwalBackup = new ObservableCollection<SaldoAwalDTO>();
            IsDataReadyToSave = false;
            SaldoAwalRows.CollectionChanged += SaldoAwalRows_CollectionChanged;
            
            Year = AppSession.SelectedYear;
            Description="Input Saldo Awal Tahun " + Year;
            LoadTreeData();

        }
        private void SaldoAwalRows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Jika ada baris baru masuk, pasang detektor perubahan properti di baris tersebut
            if (e.NewItems != null)
            {
                foreach (SaldoAwalDTO item in e.NewItems)
                {
                    item.PropertyChanged += SaldoAwalItem_PropertyChanged;
                }
            }

            // Jika ada baris dihapus, lepas detektornya agar tidak memakan memori
            if (e.OldItems != null)
            {
                foreach (SaldoAwalDTO item in e.OldItems)
                {
                    item.PropertyChanged -= SaldoAwalItem_PropertyChanged;
                }
            }

            RefreshTotals();
        }
        private void SaldoAwalItem_PropertyChanged(object sender, NotifyCollectionChangedEventArgs e)
            
        {
            // Jika user mengubah isi kolom Debet atau Kredit, hitung ulang totalnya!
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    if (item != null)
                    {
                        // Ikat event PropertyChanged ke setiap item baru
                        item.PropertyChanged += SaldoAwalItem_PropertyChanged;
                        
                    }
                }
            }

            // 2. Jika ada item yang DIHAPUS dari koleksi
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    if (item != null)
                    {
                        // Lepas ikatannya agar tidak terjadi memory leak
                        item.PropertyChanged -= SaldoAwalItem_PropertyChanged;
                    }
                }
            }
            
            RefreshTotals();

        }

        private void SaldoAwalItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Jika user mengubah isi kolom Debet atau Kredit, hitung ulang totalnya!
            if (e.PropertyName == nameof(SaldoAwalDTO.Debet) || e.PropertyName == nameof(SaldoAwalDTO.Credit))
            {
                RefreshTotals();
            }
        }
        private void RefreshTotals()
        {
            // Beritahu UI XAML bahwa nilai TotalDebet dan TotalCredit yang baru wajib digambar ulang
            OnPropertyChanged(nameof(TotalDebet));
            OnPropertyChanged(nameof(TotalCredit));
        }

        
        private bool _isDataReadyToSave;
        public bool IsDataReadyToSave
        {
            get => _isDataReadyToSave;
            set
            {
                if (SetProperty(ref _isDataReadyToSave, value))
                {
                    // Memberitahu tombol secara manual
                    SimpanSaldoAwalCommand.NotifyCanExecuteChanged();
                    

                }
            }
        }
        
        private void CallDataSaldoAwalAction()
        {
            LoadSaldoAwal();
            RefreshTotals();
        }

        private void OnShowDetail(AccountDTO selectedAkun)
        {
            if (selectedAkun != null)
            {
                SaldoAwalDTO saldoAwalDTO = new SaldoAwalDTO
                {
                    Id=0,
                    AccountCode = selectedAkun.Id,
                    AccountName = selectedAkun.Name,
                    Debet = 0,
                    Credit = 0,
                };
                SaldoAwalRows.Add(saldoAwalDTO);
                IsDataReadyToSave = SaldoAwalRows.Any();
                
                }
          
        }
        public ObservableCollection<AccountDTO> AkunTreeList
        {
            get => _akunTreeList;
            set { _akunTreeList = value; OnPropertyChanged(); }
        }
        public AccountDTO GetSelectedAccount(IEnumerable<AccountDTO> items)
        {
            foreach (var item in items)
            {
                if (item.IsSelected) return item;
                if (item.Children != null && item.Children.Any())
                {
                    var found = GetSelectedAccount(item.Children);
                    if (found != null) return found;
                }
            }
            return null;
        }
        private async void LoadTreeData()
        {

            try
            {
                List<AccountDTO> rawData = await accountService.GetTreeAccounts();
                AkunTreeList = new ObservableCollection<AccountDTO>(rawData);

         
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail get data: {ex.Message}", "Error Sistem",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private async void LoadSaldoAwal()
        {

            try
            {
                List<SaldoAwalDTO> rawData = await saldoawalReadService.GetOnYear(Year);

                // 1. Bersihkan data lama tanpa membuang objek koleksinya
                SaldoAwalRows.Clear();
                saldoAwalBackup.Clear();
                // 2. Masukkan data baru satu per satu
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        // Setiap kali .Add() dipanggil, event CollectionChanged di constructor 
                        // otomatis menyala dan mengikat PropertyChanged untuk baris ini!
                        SaldoAwalRows.Add(item);
                    }
                }
                saldoAwalBackup = SaldoAwalRows;
                IsDataReadyToSave = SaldoAwalRows.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail get data: {ex.Message}", "Error Sistem",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private async void SimpanSaldoAwalAction()
        {
            List<SaldoAwalDTO> lst = SaldoAwalRows.ToList();
            int retValueNumOfRecord = 0;
            retValueNumOfRecord= await saldoAwalService.Save(lst);
            if (retValueNumOfRecord == 0)
            {
                MessageBox.Show($"Tidak ada data tersimpan.");
            }
            else
            {
                MessageBox.Show($"{retValueNumOfRecord} data tersimpan.");
            }

        } 

    }
}
