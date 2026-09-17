using Accounting.ViewModels.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Accounting.Views.GeneralLedger
{
    /// <summary>
    /// Interaction logic for LabaRugi.xaml
    /// </summary>
    public partial class LabaRugi : UserControl
    {
        public LabaRugi()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider.GetRequiredService<LabaRugiViewModel>(); // new ;
        }
        public DateTime? StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime? EndDate { get; set; } = DateTime.Today;

        

        private void Tampilkan_Click(object sender, RoutedEventArgs e)
        {
            if (StartDate == null || EndDate == null)
            {
                MessageBox.Show("Silakan pilih tanggal awal dan tanggal akhir.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


        }
    }
}
