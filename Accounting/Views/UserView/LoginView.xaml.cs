using Accounting.ViewModels.UserVM;
using Microsoft.Extensions.DependencyInjection;

using System.Windows;
using System.Windows.Controls;

namespace Accounting.Views.UserView
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.OnLoginSuccess += ViewModel_OnLoginSuccess;
        }

        private void ViewModel_OnLoginSuccess()
        {
            var mainView = App.ServiceProvider.GetRequiredService<MainWindow>();

            // Tampilkan Window Utama pada UI thread dan tutup login
            Dispatcher.Invoke(() =>
            {
                mainView.Show();
                this.Close();
            });
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                if (vm.LoginDto == null) vm.LoginDto = new DTO.LoginDTO();
                vm.LoginDto.Password = pb.Password;
            }
        }
    }
}
