using Accounting.DTO;
using Accounting.Services.GeneralLedgerService;
using Accounting.ViewModels.GeneralLedger;
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

namespace Accounting.Views.GeneralLedger
{
    /// <summary>
    /// Interaction logic for DetailCashFlowWindow.xaml
    /// </summary>
    public partial class DetailCashFlowWindow : Window
    {

        public DetailCashFlowWindow(CashFlowItemDTO item, CashFlowService service, Action onSaved = null)
        {
            InitializeComponent();

            var vm = new DetailAkunWindowViewModel(service, item);
            vm.RequestClose += () => this.Close();
            if (onSaved != null)
            {
                vm.RequestRefresh += onSaved;
            }

            this.DataContext = vm;
        }

        // convenience: create new item
        public DetailCashFlowWindow(CashFlowService service) : this(new CashFlowItemDTO(), service)
        {
        }

    }
}
