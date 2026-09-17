using Accounting.DTO;
using Accounting.ViewModels.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

using System.Text;

using System.Windows;
using System.Windows.Controls;

namespace Accounting.Views.GeneralLedger
{
    /// <summary>
    /// Interaction logic for LaporanView.xaml
    /// </summary>
    public partial class LaporanView : UserControl
    {
        
        public DateTime? StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime? EndDate { get; set; } = DateTime.Today;

        public LaporanView(int JenisLaporan)
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider.GetRequiredService<GeneralLedgerLaporanViewModel>(); // new ;
        }

        private void Tampilkan_Click(object sender, RoutedEventArgs e)
        {
            if (StartDate == null || EndDate == null)
            {
                MessageBox.Show("Silakan pilih tanggal awal dan tanggal akhir.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            
        }

        // Replace this with actual data loading from your repository/service
        

        
    }
}
