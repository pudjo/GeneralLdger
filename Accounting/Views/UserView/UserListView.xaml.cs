using Accounting.ViewModels.UserVM;
using Microsoft.Extensions.DependencyInjection; // Pastikan namespace ini ada
using System.Windows;
using System.Windows.Controls;


namespace Accounting.Views.UserView
{
    /// <summary>
    /// Interaction logic for UserListView.xaml
    /// </summary>
    public partial class UserListView : UserControl
    {
        public UserListView()
        {
            InitializeComponent();
            // Mengambil ViewModel langsung dari DI Container (App.xaml.cs)
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                
                this.DataContext = App.ServiceProvider.GetRequiredService<UserListViewModel>();
            }
        }
    }
}
