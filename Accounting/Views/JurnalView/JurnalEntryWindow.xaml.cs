using Accounting.ViewModels.Jurnal;
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

namespace Accounting.Views.JurnalView
{
    /// <summary>
    /// Interaction logic for JurnalEntryWindow.xaml
    /// </summary>
    public partial class JurnalEntryWindow : Window
    {
        public JurnalEntryWindow()
        {
            InitializeComponent();
        }
        private void AbaikanDanUpdateTotal(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Memaksa WPF mengirimkan angka di TextBox langsung ke properti Model saat jari mengetik
                var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                bindingExpression?.UpdateSource();

                // Panggil fungsi HitungTotal yang ada di ViewModel Bapak secara manual
                if (this.DataContext is JurnalEntryViewModel viewModel)
                {
                    // Karena fungsi HitungTotal() di ViewModel kemarin bersifat privat, 
                    // Bapak bisa mengubahnya menjadi 'public' di ViewModel lalu panggil di sini
                    viewModel.HitungTotal();
                }
            }
        }

    }
}
