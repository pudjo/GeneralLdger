using Accounting.ViewModels.Inventory;
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
using System.Windows.Shapes;

namespace Accounting.Views.Inventory
{
    /// <summary>
    /// Interaction logic for JenisPickerWindow.xaml
    /// </summary>
    public partial class JenisPickerWindow : Window
    {
        public JenisPickerWindow()
        {
            InitializeComponent();
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is JenisTreeViewModel vm && vm.Selected != null)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Silakan pilih jenis terlebih dahulu.", "Pilih Jenis", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


    }
}
