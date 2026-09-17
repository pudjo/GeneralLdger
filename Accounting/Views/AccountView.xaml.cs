using Accounting.ViewModels;
using Accounting.ViewModels.Jurnal;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Accounting.Views
{
    /// <summary>
    /// Interaction logic for AccountView.xaml
    /// </summary>
    public partial class AccountView : UserControl
    {
        public AccountView()
        {
            InitializeComponent();
            //this.DataContext = new AkunViewModel();
            this.DataContext = App.ServiceProvider.GetRequiredService<AkunViewModel>();

        }
    }
}
