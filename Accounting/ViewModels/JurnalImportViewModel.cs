using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.Services.Accounts;
using Accounting.Services.GeneralLedgerService;
using Accounting.Services.JurnalServices;
using Accounting.ViewModels.UserVM;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MiniExcelLibs;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
//using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels
{
    internal partial class JurnalImportViewModel : BaseViewModel
    {
        // 1. DAFTAR JURNAL (Manual)
        private ObservableCollection<JurnalImport> _daftarJurnal = new();
        List<JournalDetailDTO> listDetail = new List<JournalDetailDTO>();
        List<JurnalDTO> listHeader = new List<JurnalDTO>();
        JurnalService js;
        private ObservableCollection<JurnalDTO> _daftarJurnalHeader = new();
        private ObservableCollection<JournalDetailDTO> _daftarJurnalDetail = new();
        private string _kataKunciPencarian;
        private List<JurnalImport> _dataMasterExcel = new List<JurnalImport>();
        private string _sheetName = string.Empty;
        // TAMBAHAN: Command untuk Export ke Excel


        // Properti untuk mendeteksi ada yang berubah di DataGrid
        private bool _isUpdatingFromGrid = false;

        // Timer untuk debounce multiple changes
        private System.Timers.Timer _debounceTimer;
        private const int DEBOUNCE_MILLISECONDS = 300;

        public ObservableCollection<JurnalImport> DaftarJurnal
        {
            get => _daftarJurnal;
            set
            {
                if (SetProperty(ref _daftarJurnal, value))
                {
                    SubscribeToDataGridChanges();
                }
            }
        }

        public ObservableCollection<JurnalDTO> DaftarJurnalHeader
        {
            get => _daftarJurnalHeader;
            set => SetProperty(ref _daftarJurnalHeader, value);
        }

        public ObservableCollection<JournalDetailDTO> DaftarJurnalDetail
        {
            get => _daftarJurnalDetail;
            set => SetProperty(ref _daftarJurnalDetail, value);
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

        private JurnalDTO _selectedHeader;
        public JurnalDTO SelectedHeader
        {
            get => _selectedHeader;
            set
            {
                _selectedHeader = value;
                OnPropertyChanged(nameof(SelectedHeader));
                UpdateDetailTampilan();
            }
        }

        // COMMANDS
        public IRelayCommand ImportKeDatabaseCommand { get; }
        public IRelayCommand CekBukuBesarCommand { get; }
        public IRelayCommand CekKodeAkun { get; }
        public IRelayCommand ProsesKeHeaderDetailCommand { get; }
        public IRelayCommand ExportKeExcelCommand { get; }
        public ICommand CariCommand { get; }
        public ICommand ResetCariCommand { get; }

        public IRelayCommand PilihFileCommand { get; }
        private bool _isDataReady;
        public bool IsDataReady
        {
            get => _isDataReady;
            set
            {
                if (SetProperty(ref _isDataReady, value))
                {
                    ImportKeDatabaseCommand.NotifyCanExecuteChanged();
                    ProsesKeHeaderDetailCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _pesanStatus;
        public string PesanStatus
        {
            get => _pesanStatus;
            set => SetProperty(ref _pesanStatus, value);
        }

        private string _namaFileTerpilih = "Belum ada file terpilih";
        public string NamaFileTerpilih
        {
            get => _namaFileTerpilih;
            set => SetProperty(ref _namaFileTerpilih, value);
        }

        public JurnalImportViewModel()
        {
            ResetCariCommand = new RelayCommand(EksekusiReset);
            js = App.ServiceProvider.GetRequiredService<JurnalService>();
            PilihFileCommand = new RelayCommand(PilihFile);
            ImportKeDatabaseCommand = new RelayCommand(ImportKeDatabase, () => IsDataReady);
            CekBukuBesarCommand = new AsyncRelayCommand(BandingKanDenganBukubesar);
            CekKodeAkun = new AsyncRelayCommand(CekKodeAkunMethode);
            ExportKeExcelCommand = new RelayCommand(ExportKeExcel, () => DaftarJurnal?.Count > 0);

            ProsesKeHeaderDetailCommand = new RelayCommand(ProsesKeHeaderDetail, () => IsDataReady);

            // Setup debounce timer
            _debounceTimer = new System.Timers.Timer(DEBOUNCE_MILLISECONDS);
            _debounceTimer.Elapsed += (s, e) =>
            {
                _debounceTimer.Stop();
                // Panggil refresh di UI thread
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (!_isUpdatingFromGrid)
                    {
                        RefreshHeaderDetail();
                    }
                });
            };
            _debounceTimer.AutoReset = false;
        }

        //===========================
        private string _fullPathFile;

        private ObservableCollection<string> _daftarSheet = new();
        public ObservableCollection<string> DaftarSheet
        {
            get => _daftarSheet;
            set => SetProperty(ref _daftarSheet, value);
        }

        private string _sheetTerpilih;
        public string SheetTerpilih
        {
            get => _sheetTerpilih;
            set
            {
                if (SetProperty(ref _sheetTerpilih, value) && !string.IsNullOrEmpty(value))
                {
                    _sheetName = value;
                    Mouse.OverrideCursor = Cursors.Wait;

                    Task.Run(() =>
                    {
                        MuatDataDariSheet(value);
                    }).ContinueWith(t =>
                    {
                        // Setelah selesai (baik sukses atau error), kembalikan kursor ke Default di UI Thread
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            Mouse.OverrideCursor = null;
                        });
                    });
                }
            }
        }

        private bool _isSheetVisible;
        public bool IsSheetVisible
        {
            get => _isSheetVisible;
            set => SetProperty(ref _isSheetVisible, value);
        }

        private decimal _jumlahKredit;
        public decimal JumlahKredit
        {
            get => _jumlahKredit;
            set => SetProperty(ref _jumlahKredit, value);
        }

        private decimal _jumlahDebit;
        public decimal JumlahDebit
        {
            get => _jumlahDebit;
            set => SetProperty(ref _jumlahDebit, value);
        }

        private void PilihFile()
        {
            var dialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
            if (dialog.ShowDialog() == true)
            {
                _fullPathFile = dialog.FileName;
                NamaFileTerpilih = Path.GetFileName(dialog.FileName);

                try
                {
                    var names = MiniExcel.GetSheetNames(_fullPathFile);
                    DaftarSheet.Clear();
                    foreach (var name in names) DaftarSheet.Add(name);

                    IsSheetVisible = DaftarSheet.Any();
                    DaftarJurnal.Clear();
                    IsDataReady = false;
                    PesanStatus = "Silakan pilih sheet.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal membuka file: {ex.Message}");
                }
            }
        }

        private void MuatDataDariSheet(string sheetName)
        {
            try
            {
                var rows = MiniExcel.Query<JurnalImport>
                    (_fullPathFile,
                    sheetName: sheetName,
                      startCell: "A5")
                      .Where(x => !string.IsNullOrWhiteSpace(x.NoAkun) ||
                               x.Tanggal != default).ToList();

                if (!CekKolomNull(rows))
                {
                    return;
                }


                _dataMasterExcel = rows;

                // Pindahkan pembaruan properti UI ke Dispatcher (UI Thread)
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    DaftarJurnal = new ObservableCollection<JurnalImport>(_dataMasterExcel);

                    IsDataReady = DaftarJurnal.Any();
                    PesanStatus = $"Sheet [{sheetName}]: {DaftarJurnal.Count} baris ditemukan.";

                    // Jalankan refresh struktur header detail
                    RefreshHeaderDetail();
                });

                //IsDataReady = DaftarJurnal.Any();
                PesanStatus = $"Sheet [{sheetName}]: {DaftarJurnal.Count} baris ditemukan.";
                JumlahDebit = DaftarJurnal.Sum(x => x.Debit ?? 0m);
                JumlahKredit = DaftarJurnal.Sum(x => x.Kredit ?? 0m);

                //ProsesKeHeaderDetail();
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Gagal memuat sheet: {ex.Message}");
                });
            }
        }

        private bool CekKolomNull(List<JurnalImport> dataYangDice)
        {
            try
            {
                int numTrouble = ApakahAdaNullOrEmpty("Tanggal");
                if (numTrouble > 0)
                {
                    MessageBox.Show($"Kolom Tanggal ada yang kosong pada baris {numTrouble}. Silakan periksa kembali.");
                    return false;
                }
                numTrouble = ApakahAdaNullOrEmpty("No. Bukti");
                if (numTrouble > 0)
                {
                    MessageBox.Show($"Kolom No Bukti ada yang kosong pada baris {numTrouble}. Silakan periksa kembali.");
                    return false;
                }
                numTrouble = ApakahAdaNullOrEmpty("Debit");
                if (numTrouble > 0)
                {
                    MessageBox.Show($"Kolom Debet ada yang kosong atau #VALUE pada baris {numTrouble}. Silakan periksa kembali.");
                    return false;
                }
                numTrouble = ApakahAdaNullOrEmpty("Kredit");
                if (numTrouble > 0)
                {
                    MessageBox.Show($"Kolom Kredit ada yang kosong atau #VALUE pada baris {numTrouble}. Silakan periksa kembali.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terjadi kesalahan: {ex.Message}");
                System.Windows.MessageBox.Show($"Terjadi kesalahan {ex.Message}");
                return false;
            }
        }

        private int ApakahAdaNullOrEmpty(string namaHeaderTarget)
        {
            using var workbook = new XLWorkbook(_fullPathFile);

            string namaSheet = _sheetName;
            var worksheet = workbook.Worksheet(namaSheet);
            int barisHeader = 5;
            var headerRow = worksheet.Row(barisHeader);
            var headerCell = headerRow.CellsUsed(c => c.GetString().Trim() == namaHeaderTarget).FirstOrDefault();
            if (headerCell != null)
            {
                int targetColumnIndex = headerCell.Address.ColumnNumber;
                var totalRows = worksheet.LastRowUsed().RowNumber();
                totalRows = totalRows - 5;
                var dataRows = worksheet.Rows(barisHeader + 1, totalRows);

                int rowNumber = 0;
                foreach (var row in dataRows)
                {
                    if (row.IsEmpty()) continue;
                    rowNumber++;
                    var cell = row.Cell(targetColumnIndex);

                    if (cell.IsEmpty()) {
                        cell.Value = 0;

                    }
                    if (cell.Value.ToString().Contains("#VALUE"))
                    {
                        return rowNumber;
                    }
                }


                return -1;
            }
            else
            {
                System.Windows.MessageBox.Show("Ada masalah header. Cila cek");
                return -99;

            }
        }

        //============================
        /// <summary>
        /// Subscribe ke perubahan dalam DaftarJurnal dan setiap item di dalamnya.
        /// </summary>
        private void SubscribeToDataGridChanges()
        {
            if (_daftarJurnal != null)
            {
                _daftarJurnal.CollectionChanged -= DaftarJurnal_CollectionChanged;
            }

            _daftarJurnal.CollectionChanged += DaftarJurnal_CollectionChanged;

            foreach (var item in _daftarJurnal)
            {
                item.PropertyChanged -= JurnalImportItem_PropertyChanged;
                item.PropertyChanged += JurnalImportItem_PropertyChanged;
            }
        }

        private void DaftarJurnal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingFromGrid) return;

            if (e.NewItems != null)
            {
                foreach (JurnalImport item in e.NewItems)
                {
                    item.PropertyChanged -= JurnalImportItem_PropertyChanged;
                    item.PropertyChanged += JurnalImportItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (JurnalImport item in e.OldItems)
                {
                    item.PropertyChanged -= JurnalImportItem_PropertyChanged;
                }
            }

            RefreshHeaderDetail();
        }

        /// <summary>
        /// Handler untuk perubahan properti item. 
        /// Menggunakan debounce untuk menghindari refresh terlalu sering.
        /// </summary>
        private void JurnalImportItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isUpdatingFromGrid) return;

            // Debug: Catat properti yang berubah
            Debug.WriteLine($"[JurnalImportItem_PropertyChanged] PropertyName: {e.PropertyName}");

            // Hanya proses jika property yang berubah adalah yang penting
            if (e.PropertyName == nameof(JurnalImport.NoBukti) ||
                e.PropertyName == nameof(JurnalImport.Debit) ||
                e.PropertyName == nameof(JurnalImport.Kredit) ||
                e.PropertyName == nameof(JurnalImport.Keterangan) ||
                e.PropertyName == nameof(JurnalImport.NoAkun) ||
                e.PropertyName == nameof(JurnalImport.NamaAkun))
            {
                Debug.WriteLine($"[JurnalImportItem_PropertyChanged] Detected important change: {e.PropertyName}");

                // Reset dan mulai debounce timer
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }

        /// <summary>
        /// Fungsi utama yang me-refresh listHeader dan listDetail berdasarkan data DaftarJurnal terkini.
        /// </summary>
        public void RefreshHeaderDetail()
        {
            try
            {
                Debug.WriteLine("[RefreshHeaderDetail] Started");
                _isUpdatingFromGrid = true;

                // 1. Ambil data dari _dataMasterExcel (BUKAN dari DaftarJurnal)
                // Ini memastikan bahwa meskipun Anda sedang memfilter data (Pencarian),
                // listHeader dan listDetail yang baru tetap dibangun dari keseluruhan data yang utuh.
                var allData = _dataMasterExcel.ToList();

                // 2. Lakukan grouping ulang berdasarkan NoBukti yang baru saja diedit oleh user
                var groupedData = allData.GroupBy(x => x.NoBukti).ToList();

                // Siapkan list temporary baru
                var newHeader = new List<JurnalDTO>();
                var newDetail = new List<JournalDetailDTO>();

                foreach (var group in groupedData)
                {
                    // Jika user menghapus NoBukti di grid sampai kosong, abaikan sementara dari header
                    if (string.IsNullOrWhiteSpace(group.Key)) continue;

                    var firstRow = group.First();
                    // Tambahkan ke listHeader baru
                    newHeader.Add(new JurnalDTO
                    {
                        RefNo = group.Key,                     // Ini akan otomatis menjadi NoBukti yang baru
                        JournalDate = firstRow.Tanggal,
                        Description = firstRow.Keterangan,
                        TotalDebet = group.Sum(x => x.Debit ?? 0m),  // Otomatis menjumlahkan ulang item yang tersisa di grup ini
                        TotalCredit = group.Sum(x => x.Kredit ?? 0m)
                    });

                    // Tambahkan semua baris anggota grup ke listDetail baru
                    foreach (var item in group)
                    {
                        newDetail.Add(new JournalDetailDTO
                        {
                            RefNo = item.NoBukti,              // Otomatis ikut berubah
                            AccountCode = item.NoAkun,
                            AccountName = item.NamaAkun,
                            Debet = item.Debit ?? 0m,
                            Credit = item.Kredit ?? 0m
                        });

                    }
                }

                // 3. Masukkan kembali ke variabel penampung utama Anda
                listHeader = newHeader;
                listDetail = newDetail;

                Debug.WriteLine($"[RefreshHeaderDetail] Memperbarui struktur: {listHeader.Count} header dan {listDetail.Count} detail.");

                // 4. Update Tampilan UI (DaftarJurnalHeader)
                _dataMasterHeader = listHeader;
                FilterDaftarHeader(); // Memanggil ini agar fungsi "TampilkanHanyaBermasalah" tetap bekerja

                // 5. Pertahankan item yang sedang dipilih di UI jika masih ada
                if (SelectedHeader != null)
                {
                    var itemTetapAda = _dataMasterHeader.FirstOrDefault(x => x.RefNo == SelectedHeader.RefNo);
                    if (itemTetapAda != null)
                    {
                        // Jika NoBukti yang diedit adalah milik grup lain, detail tampilan kanan akan menyesuaikan otomatis
                        UpdateDetailTampilan();
                    }
                    else
                    {
                        // Jika NoBukti dari SelectedHeader lama sudah hilang/berubah, kosongkan detail atau pilih header pertama
                        DaftarJurnalDetail.Clear();
                    }
                }

                // Hitung total bawah
                JumlahDebit = allData.Sum(x => x.Debit ?? 0m);
                JumlahKredit = allData.Sum(x => x.Kredit ?? 0m);

                OnPropertyChanged(nameof(AdaDataBermasalah));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RefreshHeaderDetail] Error: {ex.Message}");
            }
            finally
            {
                _isUpdatingFromGrid = false;
            }
        }

        private async void ImportKeDatabase()
        {
            try
            {
                var validasiErrors = ValidasiDataCetak();
                if (validasiErrors.Count > 0)
                {
                    string pesan = "Data tidak valid. Silakan perbaiki error berikut:\n\n" +
                                  string.Join("\n", validasiErrors.Take(10));

                    if (validasiErrors.Count > 10)
                    {
                        pesan += $"... dan {validasiErrors.Count - 10} error lainnya.";
                    }

                    MessageBox.Show(pesan, "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<JurnalImport> lstJurnalImportEdited = DaftarJurnal.ToList();
                decimal? totaldebet = lstJurnalImportEdited.Sum(x => x.Debit);
                decimal? totalcredit = lstJurnalImportEdited.Sum(x => x.Kredit);
                if (totaldebet != null)
                {
                    MessageBox.Show($"{totaldebet} - {totalcredit}");

                }
                await js.CreateBunc(lstJurnalImportEdited);
                MessageBox.Show($"Import ke database berhasil! Total {listHeader.Count} jurnal dan {listDetail.Count} detail tersimpan.",
                               "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                //       DaftarJurnal.Clear();
                //     DaftarJurnalHeader.Clear();
                //   DaftarJurnalDetail.Clear();
                // IsDataReady = false;
                //NamaFileTerpilih = "Belum ada file terpilih";
                PesanStatus = "Impor selesai. Silakan pilih file baru.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengimpor ke database: {ex.Message}\n\n{ex.InnerException?.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task BandingKanDenganBukubesar()
        {
            try
            {
                GeneralLedgerService generalLedgerService;
                generalLedgerService = App.ServiceProvider.GetRequiredService<GeneralLedgerService>();
                DateTime startDate = DaftarJurnal.Min(x => x.Tanggal);
                DateTime endDate = DaftarJurnal.Max(x => x.Tanggal);
                var data = await generalLedgerService.GetGeneralLedgers(startDate, endDate, "");
                ObservableCollection<GeneralLedgerDTO> _generalLedger = new ObservableCollection<GeneralLedgerDTO>();
                _generalLedger = new ObservableCollection<GeneralLedgerDTO>(data);

                List<JurnalImport> lstJurnalImportEdited = DaftarJurnal.ToList();
                // jUMlah di database
                decimal jumlahDebetDatabase = _generalLedger.Sum(x => x.Debet);
                decimal jumlahCreditDatabase = _generalLedger.Sum(x => x.Credit);

                decimal? jumlahDebetExcell = lstJurnalImportEdited.Sum(x => x.Debit);
                decimal? jumlahCreditExcell = lstJurnalImportEdited.Sum(x => x.Kredit);

                // Menggunakan ?? 0m artinya: "Jika null, anggap nilainya 0"
                decimal debetExcelAman = jumlahDebetExcell ?? 0m;
                decimal creditExcelAman = jumlahCreditExcell ?? 0m;

                if (Math.Abs(debetExcelAman - jumlahDebetDatabase) < 0.001m &&
                    Math.Abs(creditExcelAman - jumlahCreditDatabase) < 0.001m)
                {
                    MessageBox.Show("Jumlah Debet dan Kredit Sesuai");
                    return;
                }
                if (jumlahDebetExcell != jumlahDebetDatabase ||
                    jumlahCreditExcell != jumlahCreditDatabase)
                {
                    if (jumlahDebetDatabase == 0 && jumlahDebetDatabase == 0)
                    {
                        MessageBox.Show("Belum di import.");
                    } else { 
                        MessageBox.Show("Jumlah Berbeda.");
                     }
                }
                else
                {


                    foreach (var item in lstJurnalImportEdited)
                    {
                        string refNo = item.NoBukti;
                        decimal? debet = lstJurnalImportEdited.Where(j => j.NoBukti == refNo).Sum(x => x.Debit);
                        decimal? kredit = lstJurnalImportEdited.Where(j => j.NoBukti == refNo).Sum(x => x.Kredit);

                        decimal? debetGL = _generalLedger.Where(j => j.RefNo == refNo).Sum(x => x.Debet);
                        decimal? kreditGL = _generalLedger.Where(j => j.RefNo == refNo).Sum(x => x.Credit);

                        if (debet != debetGL || kredit != kreditGL)
                        {
                            MessageBox.Show($"Perbedaan ditemukan untuk Ref No: {refNo}\n" +
                                            $"Jurnal Import - Debet: {debet}, Kredit: {kredit}\n" +
                                            $"Buku Besar - Debet: {debetGL}, Kredit: {kreditGL}",
                                            "Perbedaan Ditemukan", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }


            }
            catch
            {


             }
            
            
        }
        private async Task CekKodeAkunMethode()
        {
            try
            {
                AccountService accountService;
                accountService = App.ServiceProvider.GetRequiredService<AccountService>();
                List<AccountDTO> daftarAkunDatabae = await accountService.GetAccountsDTOAsync();


                // akan mengecek setiap kode akun di DaftarJurnal dan mencocokkannya dengan daftar akun dari database
                List<JurnalImport> lstJurnalImportEdited = DaftarJurnal.ToList();
                foreach ( JurnalImport  ji in lstJurnalImportEdited)
                {
                    
                    var akun = daftarAkunDatabae.FirstOrDefault(a => a.Id.Trim().ToLower() == ji.NoAkun.Trim().ToLower());
                    if (akun != null)
                    {
                        if(ji.NamaAkun.Trim().ToLower() != akun.Name.Trim().ToLower())
                        {
                            string Message = $"Nama Akun untuk {ji.NoBukti} Tanggal {ji.Tanggal.ToString("dd MMM")} berbeda.   tidak ditemukan: {ji.NoAkun} ";    
                                   Message +=$"Di Database ={akun.Name} di Excell {ji.NamaAkun}";
                            MessageBox.Show(Message, "Nama Akun Tidak Sesuai", MessageBoxButton.OK, MessageBoxImage.Warning);

                        }
                        
                    }
                    else
                    {
                        MessageBox.Show($"Kode Akun tidak ditemukan: {ji.NoAkun}\n" +
                                        $"Silakan periksa kembali.",
                                        "Kode Akun Tidak Ditemukan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    //Console.WriteLine($"Kode Akun: {akun.Id}, Nama Akun: {akun.Name}");
                }
                MessageBox.Show("Pengecekan kode akun selesai.", "Selesai", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch
            {
                // Handle exceptions if necessary
            }
        }
        private List<string> ValidasiDataCetak()
        {
            var errors = new List<string>();
            if (DaftarJurnal == null || DaftarJurnal.Count == 0)
            {
                errors.Add("Data jurnal kosong. Tidak ada data untuk diimpor.");
                return errors;
            }

            for (int i = 0; i < DaftarJurnal.Count; i++)
            {
                var row = DaftarJurnal[i];
                int rowNum = i + 1;

                if (string.IsNullOrWhiteSpace(row.NoBukti))
                    errors.Add($"Baris {rowNum}: Ref No (No. Bukti) kosong.");

                if (string.IsNullOrWhiteSpace(row.NoAkun))
                    errors.Add($"Baris {rowNum}: Kode Akun (No. Akun) kosong.");

                if (string.IsNullOrWhiteSpace(row.Keterangan))
                    errors.Add($"Baris {rowNum}: Keterangan kosong.");

                if (row.Debit < 0)
                    errors.Add($"Baris {rowNum}: Debet tidak boleh negatif (nilai: {row.Debit}).");

                if (row.Kredit < 0)
                    errors.Add($"Baris {rowNum}: Kredit tidak boleh negatif (nilai: {row.Kredit}).");

                if (row.Debit == 0 && row.Kredit == 0)
                    errors.Add($"Baris {rowNum}: Debet dan Kredit tidak boleh keduanya 0.");
            }

            return errors;
        }

        private void ProsesKeHeaderDetail()
        {
            RefreshHeaderDetail();
        }

        public bool AdaDataBermasalah
        {
            get
            {
                if (_dataMasterHeader == null || _dataMasterHeader.Count == 0) return false;
                return _dataMasterHeader.Any(x => Math.Abs(x.TotalDebet - x.TotalCredit) > 0.01m);
            }
        }

        private void UpdateDetailTampilan()
        {
            if (SelectedHeader == null) return;
            var filterDetail = listDetail.Where(x => x.RefNo == SelectedHeader.RefNo).ToList();
            DaftarJurnalDetail = new ObservableCollection<JournalDetailDTO>(filterDetail);
        }

        private void EksekusiCari()
        {
            if (string.IsNullOrWhiteSpace(KataKunciPencarian))
            {
                DaftarJurnal = new ObservableCollection<JurnalImport>(_dataMasterExcel);
            }
            else
            {
                var kataKunci = KataKunciPencarian.ToLower();
                var hasil = _dataMasterExcel.Where(x =>
                    (x.NamaAkun?.ToLower().Contains(kataKunci) ?? false) ||
                    (x.NoBukti?.ToLower().Contains(kataKunci) ?? false) ||
                    (x.Keterangan?.ToLower().Contains(kataKunci) ?? false)
                ).ToList();

                DaftarJurnal = new ObservableCollection<JurnalImport>(hasil);
            }
        }

        private void EksekusiReset()
        {
            KataKunciPencarian = string.Empty;
            DaftarJurnal = new ObservableCollection<JurnalImport>(_dataMasterExcel);
        }

        //========================================================================
        private bool _tampilkanHanyaBermasalah = false;
        public bool TampilkanHanyaBermasalah
        {
            get => _tampilkanHanyaBermasalah;
            set
            {
                if (SetProperty(ref _tampilkanHanyaBermasalah, value))
                {
                    FilterDaftarHeader();
                }
            }
        }

        private List<JurnalDTO> _dataMasterHeader = new List<JurnalDTO>();
        private void FilterDaftarHeader()
        {
            if (_dataMasterHeader == null) return;

            IEnumerable<JurnalDTO> hasil = _dataMasterHeader;

            if (TampilkanHanyaBermasalah)
            {
                hasil = _dataMasterHeader.Where(x => Math.Abs(x.TotalDebet - x.TotalCredit) > 0.01m);
            }

            DaftarJurnalHeader = new ObservableCollection<JurnalDTO>(hasil);
        }
        private void ExportKeExcel()
        {
            try
            {
                if (DaftarJurnal == null || DaftarJurnal.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diekspor.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"JurnalImport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = ".xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Tampilkan cursor loading
                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                    // Jalankan export secara async
                    Task.Run(async () =>
                    {
                        try
                        {
                            var dataToExport = DaftarJurnal.ToList();

                            // Konversi ke format yang mudah untuk di-export
                            var exportData = dataToExport.Select((item, index) => new
                            {
                                No = index + 1,
                                Tanggal = item.Tanggal.ToString("dd/MM/yyyy"),
                                RefNo = item.NoBukti,
                                KodeAkun = item.NoAkun,
                                NamaAkun = item.NamaAkun,
                                Keterangan = item.Keterangan,
                                Debet = item.Debit,
                                Kredit = item.Kredit
                            }).ToList();

                            // Tulis ke Excel menggunakan MiniExcel
                            await Task.Run(() =>
                            {
                                MiniExcel.SaveAs(saveDialog.FileName, exportData);
                            });

                            Application.Current?.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(
                                    $"Export berhasil! File disimpan di:\n{saveDialog.FileName}",
                                    "Sukses",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                                System.Windows.Input.Mouse.OverrideCursor = null;
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.Current?.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Gagal mengekspor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                System.Windows.Input.Mouse.OverrideCursor = null;
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

    }
}