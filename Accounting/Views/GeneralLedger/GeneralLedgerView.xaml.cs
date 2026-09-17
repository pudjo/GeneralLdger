
using Accounting.DTO;
using Accounting.Services.GeneralLedgerService;
using Accounting.ViewModels.Akun;
using Accounting.ViewModels.GeneralLedger;
using Accounting.ViewModels.Jurnal;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace Accounting.Views.GeneralLedger
{
    /// <summary>
    /// Interaction logic for GeneralLedgerView.xaml
    /// </summary>
    public partial class GeneralLedgerView : UserControl
    {
        public GeneralLedgerView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider.GetRequiredService<GeneralLedgerViewModel>(); 
        }
        public AccountDTO AkunTerpilih { get; private set; }


        private AccountDTO CariYangDipilih(IEnumerable<AccountDTO> nodes)
        {
            GeneralLedgerViewModel dc = (GeneralLedgerViewModel)this.DataContext;
            return dc.SelectedAkun;

        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = (GeneralLedgerViewModel)this.DataContext;
            vm.SelectedAkun = e.NewValue as AccountDTO;
        }

    }
}
