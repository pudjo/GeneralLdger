using Accounting.DTO;
using Accounting.Services.JurnalServices;
using Accounting.Views.Account;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClosedXML.Excel;
using System.Collections.Specialized;
using static System.Net.Mime.MediaTypeNames;



namespace Accounting.ViewModels.Jurnal
{
    internal class JurnalEntryViewModel:BaseViewModel
    {
        private JurnalService _jurnalService ;
        private JurnalDTO _header;
        public JurnalDTO Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private decimal _totalDebet;
        public decimal TotalDebet
        {
            get => _totalDebet;
            set => SetProperty(ref _totalDebet, value);
        }

        private decimal _totalKredit;
        public decimal TotalKredit
        {
            get => _totalKredit;
            set => SetProperty(ref _totalKredit, value);
        }
        private decimal _selisih;
        public decimal Selisih
        {
            get => _selisih;
            set
            => SetProperty(ref _selisih, value);
                
                
            
        }
        private Brush _selisihColor;
        public Brush SelisihColor
        {
            get => _selisihColor;
            set
            => SetProperty(ref _selisihColor, value);



        }

        private ObservableCollection<JournalDetailDTO> _daftarDetail;
        public ObservableCollection<JournalDetailDTO> DaftarDetail
        {
            get => _daftarDetail;
            set
            {
                if (_daftarDetail != value)
                {
                    // Lepas event listener dari koleksi lama jika ada
                    if (_daftarDetail != null)
                        _daftarDetail.CollectionChanged -= OnDaftarDetailChanged;

                    _daftarDetail = value;
                    OnPropertyChanged(nameof(DaftarDetail));

                    // Pasang event listener ke koleksi baru
                    if (_daftarDetail != null)
                        _daftarDetail.CollectionChanged += OnDaftarDetailChanged;

                    HitungTotal();
                }
            }
        }


        // for new Journal
        // Entry, we need to initialize with default values
        public JurnalEntryViewModel( )
        {
            // Get th e service for database operatioan
            _jurnalService = App.ServiceProvider.GetRequiredService<JurnalService>();

            // Initialize for the new Journal.Shono parameter 
            // Create new empty Journal
            Header = new JurnalDTO
            {
                JournalDate = DateTime.Now,
                RefNo = "" 
               
            };
            DaftarDetail = new ObservableCollection<JournalDetailDTO>();

            // ads two Empty lines of detil
            DaftarDetail.Add(new JournalDetailDTO());
            DaftarDetail.Add(new JournalDetailDTO());
        }
        // for display existing journal, we can have another constructor that accepts JurnalDTO and List<JournalDetailDTO>
        // New constructor: instantiate viewmodel for editing an existing jurnal
        public JurnalEntryViewModel(JurnalDTO header )
        {
            _jurnalService = App.ServiceProvider.GetRequiredService<JurnalService>();

            if (header == null)
            {
                // fallback ke konstruktor default
                Header = new JurnalDTO
                {
                    JournalDate = DateTime.Now,
                    RefNo = ""
                };
                DaftarDetail = new ObservableCollection<JournalDetailDTO>();
                DaftarDetail.Add(new JournalDetailDTO());
                DaftarDetail.Add(new JournalDetailDTO());
                return;
            }

            // Copy the prameter object to Journal 
            Header = new JurnalDTO
            {
                JournalDate = header.JournalDate,
                RefNo = header.RefNo,
                Description = header.Description
                // Tambahkan properti lain jika DTO Anda punya lebih banyak field yang perlu disalin
            };

            // Clone setiap detail agar perubahan di form tidak langsung mengubah sumber
            var cloned = (header.Detail ?? new System.Collections.Generic.List<JournalDetailDTO>())
                .Select(d => new JournalDetailDTO
                {
                    AccountCode = d.AccountCode,
                    AccountName = d.AccountName,
                    Debet = d.Debet,
                    Credit = d.Credit
                    // Tambahkan properti lain jika ada
                }).ToList();

            DaftarDetail = new ObservableCollection<JournalDetailDTO>(cloned);
            // Tidak menambahkan baris kosong ekstra pada mode edit
        }

        
        private void OnDaftarDetailChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (JournalDetailDTO item in e.NewItems)
                {
                    // Pasang pendengar perubahan angka jika user mengetik di baris baru
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (JournalDetailDTO item in e.OldItems)
                {
                    // Lepas pendengar jika baris dihapus agar tidak bocor memori
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            HitungTotal();
        }

        // Mengurus deteksi setiap kali user mengetik angka di kolom Debit atau Kredit
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(JournalDetailDTO.Debet) ||
                e.PropertyName == nameof(JournalDetailDTO.Credit))
            {
                HitungTotal();
            }
        }

        // Fungsi inti kalkulasi matematika jurnal
        public void HitungTotal()
        {
            TotalDebet = DaftarDetail?.Sum(x => x.Debet) ?? 0;
            TotalKredit = DaftarDetail?.Sum(x => x.Credit) ?? 0;
            Selisih = Math.Abs(TotalDebet - TotalKredit);
            SelisihColor = Selisih == 0 ? Brushes.Green : Brushes.Red;
            
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public ICommand CariAkunCommand => new RelayCommand<object>(EksekusiCariAkun);
        
        private void EksekusiCariAkun(object parameter)
        {
            // Cek apakah parameter yang dikirim adalah baris detail
            if (parameter is JournalDetailDTO detailBaris)
            {
                var winPilihAkun = new AkunSelectionWindow();

                // Atur Owner agar muncul di tengah form input (opsional tapi bagus)
               // winPilihAkun.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

                if (winPilihAkun.ShowDialog() == true)
                {
                    var akunDipilih = winPilihAkun.AkunTerpilih;

                    if (akunDipilih != null)
                    {
                        // Masukkan data ke baris yang diklik
                        detailBaris.AccountCode= akunDipilih.Id;
                        detailBaris.AccountName = akunDipilih.Name;
                        DaftarDetail.Add(new JournalDetailDTO());
                    }
                }
            }
        }




        public ICommand SimpanCommand => new RelayCommand<object>(EksekusiSimpan);

        private void EksekusiSimpan(object parameter)
        {
            // 1. Validasi Data sebelum Simpan
            
            if (string.IsNullOrEmpty(Header.RefNo))
            {
                MessageBox.Show("Nomor Bukti harus diisi, Pak.");
                return;
            }
            
            if (Selisih != 0)
            {
                MessageBox.Show("Jurnal belum balance! Selisih masih ada.");
                return;
            }
            
            try
            {
                Header.Detail = _daftarDetail.ToList();
                _jurnalService.Simpan(Header);
                 MessageBox.Show("Data Jurnal berhasil disimpan!");
                 // 3. Baru setelah sukses, tutup jendela
                  if (parameter is Window window)
                  {
                   window.DialogResult = true;
                   window.Close();
                }

                // 2. Proses Simpan ke Database
                // Misalkan Bapak punya JurnalService
                //bool sukses = _jurnalService.SimpanJurnal(Header, DaftarDetail);

                //if (sukses)
                //{
                //  MessageBox.Show("Data Jurnal berhasil disimpan!");

                // 3. Baru setelah sukses, tutup jendela
                //  if (parameter is Window window)
                // {
                //   window.DialogResult = true;
                //  window.Close();
                //  }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Waduh, gagal simpan Pak: {ex.Message}");
            }

        }
    }
}
