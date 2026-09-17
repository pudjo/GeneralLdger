using Accounting.ViewModels;

using System.Windows.Controls;

namespace Accounting.Views
{
    /// <summary>
    /// Interaction logic for CompanySettingView.xaml
    /// </summary>
    public partial class CompanySettingView : UserControl
    {
        public CompanySettingView()
        {
            InitializeComponent();
            this.DataContext = new CompanySettingViewModel();
        }
    }
}
