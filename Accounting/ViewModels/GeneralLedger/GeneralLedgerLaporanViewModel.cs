using Accounting.Domain;
using Accounting.DTO;
using Accounting.Report;
using Accounting.Services.Accounts;
using Accounting.Services.GeneralLedgerService;
using Accounting.ViewModels.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Accounting.ViewModels.GeneralLedger
{
    internal class GeneralLedgerLaporanViewModel : BaseViewModel
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
        public DateTime Tanggal
        {
            get => _tangggal;
            set
            {
                _tangggal = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadGeneralLedgerCommand => new RelayCommand(() => LoadBukuBesar());
        public ICommand CetakPdfCommand => new RelayCommand(() => CetakPDF());

        public GeneralLedgerLaporanViewModel(
            GeneralLedgerService _generalLedgerService)
        {
            generalLedgerService = _generalLedgerService;
            
            if (AppSession.SelectedYear < DateTime.Now.Year)
                Tanggal = new DateTime(AppSession.SelectedYear, 12, 31);
            else
                Tanggal = DateTime.Now.Date;

        }

        /// <summary>
        /// Load data Buku Besar berdasarkan range tanggal yang dipilih
        /// </summary>
        private async void LoadBukuBesar()
        {
            try
            {
                // Gunakan method GetFirstDate dan GetEndDate dari DateRangePickerViewModel
                
                DateTime end = Tanggal;
                List<GeneralLedgerLaporanDTO> laporan = await generalLedgerService.GetBalanceSheet(end);
                if (jenisLaporan == 0)
                {

                    ListLaporan = new ObservableCollection<GeneralLedgerLaporanDTO>(
                        laporan.Where(x => x.AccountCode != null &&
                      (x.AccountCode.StartsWith("1") ||
                       x.AccountCode.StartsWith("2") ||
                       x.AccountCode.StartsWith("3"))));
                } else
                {
                    ListLaporan = new ObservableCollection<GeneralLedgerLaporanDTO>(
laporan.Where(x => x.AccountCode != null &&
  (x.AccountCode.StartsWith("4") ||
   x.AccountCode.StartsWith("5") ||
   x.AccountCode.StartsWith("6"))));
                }
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
                var document = new GeneralLedgerPdfDocument(new List<GeneralLedgerLaporanDTO>(ListLaporan), Tanggal);
                

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

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // Jika user memilih "Ya", tampilkan SaveFileDialog untuk memilih tempat penyimpanan permanen
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF Files (*.pdf)|*.pdf",
                        FileName = $"BukuBesar_{Tanggal:yyyyMMdd}.pdf"
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

