using Accounting.DTO;
using Accounting.Report;
using Accounting.Services.GeneralLedgerService;
using Accounting.ViewModels.Controls;
using CommunityToolkit.Mvvm.Input;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using QuestPDF.Fluent;
using System.IO;
using Accounting.Services;
using Accounting.Domain;
namespace Accounting.ViewModels.GeneralLedger
{
    internal class LabaRugiViewModel : BaseViewModel
    {
        private readonly int jenisLaporan;
        private GeneralLedgerService generalLedgerService;
        private ObservableCollection<GeneralLedgerLaporanDTO> _listLaporan;
        private DateRangePickerViewModel _dateRangePicker;

        public ObservableCollection<GeneralLedgerLaporanDTO> ListLaporan
        {
            get => _listLaporan;
            set
            {
                _listLaporan = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ViewModel untuk DateRangePicker control
        /// </summary>
        private DateTime _tangggal;
        public DateTime TanggalAkhir
        {
            get => _tangggal;
            set
            {
                _tangggal = value;
                OnPropertyChanged();
            }
        }
        
        private DateTime _tangggalAwal;
        public DateTime TanggalAwal
        {
            get => _tangggalAwal;
            set
            {
                _tangggalAwal = value;
                OnPropertyChanged();
            }
        }
        public ICommand LoadGeneralLedgerCommand => new RelayCommand(() => LoadBukuBesar());
        public ICommand CetakPdfCommand => new RelayCommand(() => CetakPDF());

        public LabaRugiViewModel(
            GeneralLedgerService _generalLedgerService)
        {
            generalLedgerService = _generalLedgerService;

            TanggalAwal = new DateTime(AppSession.SelectedYear ,1, 1);
            if (AppSession.SelectedYear < DateTime.Now.Year)
                TanggalAkhir = new DateTime(AppSession.SelectedYear, 12, 31);
            else 
                TanggalAkhir = DateTime.Now.Date;
        }

        /// <summary>
        /// Load data Buku Besar berdasarkan range tanggal yang dipilih
        /// </summary>
        private async void LoadBukuBesar()
        {
            try
            {
                DateTime end = TanggalAkhir;
                DateTime start =TanggalAwal;
                List<GeneralLedgerLaporanDTO> laporan = await generalLedgerService.GetLabaRugi(start,end);
                
                
                    ListLaporan = new ObservableCollection<GeneralLedgerLaporanDTO>(
                        laporan.Where(x => x.AccountCode != null &&
                          (x.AccountCode.StartsWith("4") ||
                            x.AccountCode.StartsWith("5") ||
                            x.AccountCode.StartsWith("6"))));
                
            }
            catch (Exception ex)
            {
                // Handle error (e.g., show message to user)
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void CetakPDF()
        {
            try
            {
                if (ListLaporan == null || ListLaporan.Count == 0)
                {
                    System.Windows.MessageBox.Show("Tidak ada data untuk dicetak.", "Informasi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Set lisensi QuestPDF
                QuestPDF.Settings.License = LicenseType.Community;

                // Buat objek dokumen PDF
                var document = new LabaRugiPdfDocument(new List<GeneralLedgerLaporanDTO>(ListLaporan),
                                                        TanggalAwal,TanggalAkhir);
                
                
                

                // Buat path file temporary yang unik
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"BukuBesar_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                // Generate PDF ke file temporary
                document.GeneratePdf(tempFilePath);

                // Buka file PDF menggunakan aplikasi default sistem (Edge / Adobe Reader)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = true
                });
                // 3. Tampilkan Kotak Konfirmasi (MessageBox) kepada pengguna
                var result = System.Windows.MessageBox.Show(
                    "Apakah Anda ingin menyimpan laporan PDF ini ke komputer Anda?",
                    "Konfirmasi Simpan Laporan",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);
                DateTime now = DateTime.Now;
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // Jika user memilih "Ya", tampilkan SaveFileDialog untuk memilih tempat penyimpanan permanen
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        
                        Filter = "PDF Files (*.pdf)|*.pdf",
                        FileName = $"LabaRugi_{TanggalAwal:yyyyMMdd}sd{TanggalAkhir:yyyyMMdd}_at_{now:yyMMM HHmm}.pdf"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Salin dari file temporary ke lokasi yang dipilih user
                        File.Copy(tempFilePath, saveFileDialog.FileName, overwrite: true);
                        System.Windows.MessageBox.Show("Laporan berhasil disimpan!", "Sukses", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Gagal membuka preview PDF: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
