using Accounting.Domain;

using Accounting.DTO;

using Accounting.Services.JurnalServices;
using Accounting.Views.JurnalView;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Diagnostics;

using System.Windows;

using System.Windows.Input;

namespace Accounting.ViewModels.Jurnal
{
    internal class JurnalViewModel: BaseViewModel
    {
        // Properti Command untuk tombol Tambah
        public ICommand TampilkanDataCommand => new RelayCommand(ExecuteShowJurnals);
        public ICommand TambahJurnalCommand => new RelayCommand(EksekusiTambahJurnal);
        public ICommand EditJurnalCommand => new RelayCommand<object>(EditExecution);

        // TAMBAHAN: Command untuk Hapus Jurnal Terpilih
        public ICommand HapusJurnalTerpilihCommand => new RelayCommand(OnHapusJurnalTerpilih);

        JurnalReadService service;
        IJurnalService js;
        private List<JurnalDTO> _journalData = new List<JurnalDTO>();
        private DateTime tglAwal;
        public DateTime TglAwal
        {
            get => tglAwal;
            set
            {
                tglAwal = value;
                OnPropertyChanged();
            }
        }
        private DateTime tglAkhir;
        public DateTime TglAkhir
        {
            get => tglAkhir;
            set
            {
                tglAkhir = value;
                OnPropertyChanged();
            }
        }


        private bool _isReplacePanelOpen;
        public bool IsReplacePanelOpen
        {
            get => _isReplacePanelOpen;
            set
            {
                _isReplacePanelOpen = value;
                OnPropertyChanged(nameof(IsReplacePanelOpen));
            }
        }

        private string _textToFind;
        public string TextToFind
        {
            get => _textToFind;
            set
            {
                _textToFind = value;
                OnPropertyChanged(nameof(TextToFind));

                RefreshReplaceCommand();
            }
        }

        private string _textToReplace;
        public string TextToReplace
        {
            get => _textToReplace;
            set
            {
                _textToReplace = value;
                OnPropertyChanged(nameof(TextToReplace));
            }
        }


        private ICommand _toggleReplacePanelCommand;
        public ICommand ToggleReplacePanelCommand { get; }
        private ICommand _executeReplaceCommand;
        private void RefreshReplaceCommand()
        {
            if (ExecuteReplaceCommand is RelayCommand relayCmd)
            {
                relayCmd.NotifyCanExecuteChanged();
             }
            else
            {
                // Jika RelayCommand Anda standar WPF tanpa library external
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
        public ICommand ExecuteReplaceCommand
        {
            get => _executeReplaceCommand;
            set
            {
                _executeReplaceCommand = value;
                OnPropertyChanged(nameof(ExecuteReplaceCommand));
            }
        }

        public JurnalViewModel(JurnalReadService _service,
                             IJurnalService _js )
        {
            IsReplacePanelOpen = false;
            ToggleReplacePanelCommand = new RelayCommand(OnToggleReplacePanel);
            ExecuteReplaceCommand = new RelayCommand(OnExecuteReplace, CanExecuteReplace);
            _js.JurnalChanged += OnJurnalChangedHandler;
            TglAwal = new DateTime(AppSession.SelectedYear, 1, 1);
            TglAkhir= new DateTime(AppSession.SelectedYear, 12, 31);

            OnPropertyChanged(nameof(ExecuteReplaceCommand));
            SearchCommand =  new RelayCommand(SearchExcecution);
            service = _service;
            js = _js;    
        }
        private async void OnJurnalChangedHandler (object? sender, EventArgs e)
        {
            // Pastikan eksekusi UI thread jika diperlukan, atau langsung panggil LoadTreeData()
            LoadJournalAsync();
        }

        // --- TAMBAHAN LOGIKA PILIH SEMUA ---
        private bool _isAllSelected;
        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (SetProperty(ref _isAllSelected, value))
                {
                    // Jika "Pilih Semua" dicentang/dilepas, samakan status seluruh item di grid
                    if (ListUniqueJournal != null)
                    {
                        foreach (var jurnal in ListUniqueJournal)
                        {
                            jurnal.IsSelected = value;
                        }
                    }
                }
            }
        }

        private async void OnHapusJurnalTerpilih()
        {
            if (ListUniqueJournal == null || !ListUniqueJournal.Any()) return;

            // Filter item yang dicentang
            List< JurnalDTO> itemTerpilih = ListUniqueJournal.Where(x => x.IsSelected).ToList();

            if (!itemTerpilih.Any())
            {
                MessageBox.Show("Silahkan pilih (centang) jurnal yang ingin dihapus terlebih dahulu.",
                                "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                
                return;
            }
          


            
            
            var konfirmasi = MessageBox.Show($"Apakah Anda yakin ingin menghapus {itemTerpilih.Count} jurnal yang dipilih?",
                                                 "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (konfirmasi == MessageBoxResult.Yes)
            {
                try
                {
                    bool ret = await js.DeleteBunc(itemTerpilih);
                    if (ret)
                    {
                        _isAllSelected = false;
                        OnPropertyChanged(nameof(IsAllSelected));

                        MessageBox.Show("Jurnal terpilih berhasil dihapus.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);


                    }
                    else
                    {
                        MessageBox.Show($"Penghapusan gagal! Silahkan cek log error.",
                                       "Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                    // Reset status CheckBox utama
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal menghapus jurnal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OnToggleReplacePanel()
        {
            IsReplacePanelOpen = !IsReplacePanelOpen;
        }
        private bool CanExecuteReplace()
        {
            
            return !string.IsNullOrWhiteSpace(TextToFind);
        }
        private void OnExecuteReplace()
        {
            if (ListUniqueJournal == null || !ListUniqueJournal.Any())
            {
                MessageBox.Show("Tidak ada data jurnal yang bisa diproses.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validasi string kosong untuk pengaman tambahan
            string find = TextToFind ?? string.Empty;
            string replace = TextToReplace ?? string.Empty;

            int jumlahTerubah = 0;

            // Lakukan perulangan untuk mengganti teks keterangan di UI (Koleksi DataGrid)
            foreach (var jurnal in ListUniqueJournal)
            {
                if (jurnal.Description != null && jurnal.Description.Contains(find, StringComparison.OrdinalIgnoreCase))
                {
                    // Lasskukan replace teks
                    jurnal.Description = jurnal.Description.Replace(find, replace, StringComparison.OrdinalIgnoreCase);

                    // Beritahu DataGrid bahwa properti Description di baris ini berubah (Jika model mengimplementasikan INotifyPropertyChanged)
                    // Atau jika model Anda tidak punya INotifyPropertyChanged, DataGrid akan terupdate otomatis di langkah refresh bawah.
                    jumlahTerubah++;
                }
            }

            if (jumlahTerubah > 0)
            {
                // REFRESH DATA GRID: Jika model Jurnal Anda tidak memiliki properti internal yang reaktif,
                // trik memicu pembaruan UI massal tercepat adalah dengan me-refresh referensi koleksinya.
                var tempCollection = new ObservableCollection<JurnalDTO>(ListUniqueJournal);
                ListUniqueJournal = tempCollection;

                MessageBox.Show($"{jumlahTerubah} data keterangan jurnal berhasil diperbarui di layar.\nJangan lupa simpan perubahan jika diperlukan.",
                                "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                // Opsional: Bersihkan form setelah sukses
                TextToFind = string.Empty;
                TextToReplace = string.Empty;
                IsReplacePanelOpen = false; // Tutup panel kembali
            }
            else
            {
                MessageBox.Show($"Teks '{find}' tidak ditemukan pada kolom keterangan jurnal mana pun.",
                                "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private List<JournalDetailDTO> lstDetail;
        public List<JournalDetailDTO> LstDetail
        {
            set => SetProperty(ref lstDetail, value);
            get => lstDetail;

        }

        private string whatToSearch;
        public string WhatToSearch
        {


            get => whatToSearch;
            set
            {
                if (SetProperty(ref whatToSearch, value))
                {

                    SearchExcecution();
                }
            }
        }
        ICommand SearchCommand;
        
        private void SearchExcecution()
        {
            
            if (string.IsNullOrWhiteSpace(WhatToSearch))
            {
                // Jika kosong, kembalikan ke data asli
                ListUniqueJournal = new ObservableCollection<JurnalDTO>(_journalData);
            }
            else
            {
                var keyword = WhatToSearch.ToLower();
                var result = _journalData.Where(x =>
                    (x.RefNo?.ToLower().Contains(keyword) ?? false) ||
                    (x.Description?.ToLower().Contains(keyword) ?? false)
                ).ToList();

                ListUniqueJournal = new ObservableCollection<JurnalDTO>(result);
            }
        }
        private ObservableCollection<JurnalDTO> _listUniqueJournal;
        public ObservableCollection<JurnalDTO> ListUniqueJournal
        {
            
            set => SetProperty(ref _listUniqueJournal, value);  
            get { return _listUniqueJournal; }  

        }
        
        
        // Load data
        private async Task LoadJournalAsync()
        {
            try
            {
                
                var data = await service.GetJurnalAndDetail(TglAwal, TglAkhir, 0);
                ListUniqueJournal = new ObservableCollection<JurnalDTO>(data);
                _journalData = data;


            }
            catch (Exception ex)
            {
                // Tangani error atau log di sini jika proses load gagal
                Debug.WriteLine($"Gagal memuat data user: {ex.Message}");
            }
        }

        private JurnalDTO _selectedHeader;
        public JurnalDTO SelectedHeader
        {
            get => _selectedHeader;
            set
            {
                _selectedHeader = value;
                OnPropertyChanged(nameof(SelectedHeader));
                // Setiap kali Baris Jurnal diklik, kita update isi Detail
                UpdateDetailTampilan();
            }
        }

        
        private void UpdateDetailTampilan()
        {
            if (SelectedHeader == null) return;
            
            List<JournalDetailDTO> lstDetail = new List<JournalDetailDTO>();
            LstDetail = SelectedHeader.Detail;
           
        }
        private void ExecuteShowJurnals()
        {
            LoadJournalAsync();
        }
        private void EksekusiTambahJurnal()
        {
            try
            {
                
                var formInput = new JurnalEntryWindow();

                // 2. Buat ViewModel khusus untuk form tersebut
                // Kita buat baru agar datanya kosong (siap input baru)
                var vmInput = new JurnalEntryViewModel();

                // 3. Pasangkan ViewModel ke Window
                formInput.DataContext = vmInput;

                bool? hasil = formInput.ShowDialog();

                if (hasil == true)
                {
                    // Jika user klik Simpan (DialogResult = true), 
                    // maka refresh daftar jurnal di Kolom 2 Layar Utama
                    //  TampilkanDataKeGrid();
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void EditExecution(object parameter)
        {
            try
            {
                // parameter diikat di XAML CommandParameter="{Binding}" sehingga merupakan JurnalDTO
                var selected = parameter as JurnalDTO ?? SelectedHeader;
                if (selected == null) return;

                var formInput = new JurnalEntryWindow();

                // Buat ViewModel untuk edit, pass DTO (constructor akan clone data)
                var vmInput = new JurnalEntryViewModel(selected);

                // 3. Pasangkan ViewModel ke Window
                formInput.DataContext = vmInput;

                bool? hasil = formInput.ShowDialog();

                if (hasil == true)
                {
                    // Jika user klik Simpan (DialogResult = true), 
                    // maka refresh daftar jurnal di Kolom 2 Layar Utama
                    //  TampilkanDataKeGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
               
    }
}
