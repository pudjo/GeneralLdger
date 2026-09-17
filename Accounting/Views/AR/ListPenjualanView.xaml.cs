using Accounting.DTO.AR;
using Accounting.ViewModels.AR;
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

namespace Accounting.Views.AR
{
    /// <summary>
    /// Interaction logic for ListPenjualanView.xaml
    /// </summary>
    public partial class ListPenjualanView : UserControl
    {
        internal ListPenjualanView(ListPenjualanViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.ShowDetailRequested += Vm_ShowDetailRequested;
        }

        private void Vm_ShowDetailRequested(object? sender, PenjualanDTO e)
        {
            if (e == null) return;

            // Simple preview — replace with full form navigation in your app
            var msg = $"Penjualan Id: {e.Id}\nTanggal: {e.Tanggal}\nCustomer: {e.CUstomerName}\nDetails: {e.Details?.Count ?? 0} items";
            MessageBox.Show(msg, "Penjualan Detail", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
