using System.Security;
using System.Windows;
using System.Windows.Controls;


namespace Accounting.Views
{
    /// <summary>
    /// Interaction logic for BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
        // 1. Daftarkan Dependency Property agar bisa di-Bind dari Window luar
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
            
        }
        // 2. Ketika ViewModel berubah (misal dikosongkan dari luar), update UI PasswordBox-nya
        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BindablePasswordBox passwordBox)
            {
                // Mencegah looping infinite update
                if (passwordBox.txtPassword.Password != (string)e.NewValue)
                {
                    passwordBox.txtPassword.Password = (string)e.NewValue;
                }
            }
        }

        // 3. Ketika user mengetik di UI, langsung lempar nilainya ke properti Password (ViewModel)
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = txtPassword.Password;
        }
    }
}
