using Accounting.DTO;
using Accounting.ViewModels;
using Accounting.ViewModels.Akun;
using Accounting.ViewModels.Jurnal;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Accounting.Views.Account
{
    /// <summary>
    /// Interaction logic for AkunSelectionWindow.xaml
    /// </summary>
    public partial class AkunSelectionWindow : Window
    {
        private bool canSelectAllAccountCode = false;
        public AkunSelectionWindow(bool _canSelectAllAccountCode = false)
        {
            InitializeComponent();
            //this.DataContext = new AkunSelectionWindowVewModel();
            this.DataContext = App.ServiceProvider.GetRequiredService<AkunSelectionWindowVewModel>();
            this.canSelectAllAccountCode = _canSelectAllAccountCode;
        }


        public AccountDTO AkunTerpilih { get; private set; }

        private void BtnPilih_Click(object sender, RoutedEventArgs e)
        {
            var vm = (AkunSelectionWindowVewModel)this.DataContext;
            // Cari di dalam koleksi AkunTreeList mana yang IsSelected-nya True
            // Kita bisa buat fungsi pencarian rekursif
            var terpilih = CariYangDipilih(vm.AkunTreeList);

            if (terpilih != null)
            {
                this.AkunTerpilih = terpilih;
                
                if (this.AkunTerpilih.Children.Any()== true && 
                    this.canSelectAllAccountCode == false)
                {
                    MessageBox.Show("Bukan akun paling detail");
                    return;
                }
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Silakan pilih akun terlebih dahulu.");
            }
        }

        private AccountDTO CariYangDipilih(IEnumerable<AccountDTO> nodes)
        {
            AkunSelectionWindowVewModel dc = (AkunSelectionWindowVewModel)this.DataContext;
            return dc.SelectedAkun;
        
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = (AkunSelectionWindowVewModel)this.DataContext;
            vm.SelectedAkun = e.NewValue as AccountDTO;
        }



    }
}
