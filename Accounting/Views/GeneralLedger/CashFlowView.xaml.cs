
using Accounting.ViewModels;
using Accounting.ViewModels.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Accounting.Views.GeneralLedger
{
    /// <summary>
    /// Interaction logic for CashFlowView.xaml
    /// </summary>
    public partial class CashFlowView : UserControl
    {
        public CashFlowView()
        {
            InitializeComponent();
            //this.DataContext = new AkunViewModel();
            this.DataContext = App.ServiceProvider.GetRequiredService<CashFlowItemViewModel>();
        }
    }
}
