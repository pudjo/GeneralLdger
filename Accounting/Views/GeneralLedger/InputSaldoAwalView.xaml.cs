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
    /// Interaction logic for InputSaldoAwalView.xaml
    /// </summary>
    public partial class InputSaldoAwalView : UserControl
    {
        public InputSaldoAwalView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider.GetRequiredService<InputSaldoAwalViewModel>();
        }
        
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Filter klik ganda tombol kiri mouse dilakukan di sini sebelum masuk ke ViewModel
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (sender is StackPanel panel && panel.DataContext != null)
                {
                    // Ambil data objek Akun dari DataContext baris yang di-klik
                    var dataAkun = panel.DataContext;

                    // Ambil MainViewModel utama dari DataContext milik Window/UserControl Anda
                    var viewModel = this.DataContext as InputSaldoAwalViewModel; // Sesuaikan dengan nama ViewModel Anda

                    if (viewModel != null && viewModel.ShowDetailCommand.CanExecute(dataAkun))
                    {
                        // Eksekusi command dan kirim dataAkun sebagai parameter
                        viewModel.ShowDetailCommand.Execute(dataAkun);

                        // Tandai event telah selesai ditangani agar tidak memicu double-click bertingkat
                        e.Handled = true;
                    }
                }
            }
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Tetap biarkan jika ada logika sinkronisasi bawaan Anda
        }

        private void BtnPilih_Click(object sender, RoutedEventArgs e)
        {
            // Tombol pilih manual (bisa memicu fungsi double klik juga)
           // TreeView_MouseDoubleClick(treeRekening, null);
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            // Logika untuk menyimpan isi objek FormSaldoAwalList ke dalam Database
            MessageBox.Show("Data Saldo Awal Berhasil Diproses ke Database!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    }

