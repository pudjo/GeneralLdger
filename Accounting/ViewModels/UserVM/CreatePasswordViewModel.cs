using Accounting.Domain;
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.Services.Administration;
using Accounting.Services.JurnalServices;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.UserVM
{
    internal class CreatePasswordViewModel:BaseViewModel
    {
        private string _password;
        private string _confirmPassword;
        private bool _isPasswordMatch;
        private UserService service;
        // Properti Data User (Bisa diisi saat form di-load)
        public static string LoggedInUserID { get; set; }
        public static string LoggedInUserName { get; set; }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ValidasiPassword();
                }
            }
        }
        private UserDTO selecteduser;
        public UserDTO SelectedUser
        {
            get => selecteduser;
            set { selecteduser = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ValidasiPassword();
                }
            }
        }

        // Properti ini yang dibaca oleh XAML untuk membuat border merah
        public bool IsPasswordMatch
        {
            get => _isPasswordMatch;
            private set => SetProperty(ref _isPasswordMatch, value);
        }

        public ICommand SimpanPasswordCommand { get; }

        public CreatePasswordViewModel(UserDTO _userDTO )
        {
            SelectedUser = _userDTO;
            
            service = App.ServiceProvider.GetRequiredService<UserService>();
            // Tombol simpan hanya Aktif (true) jika IsPasswordMatch bernilai true
            SimpanPasswordCommand = new RelayCommand(ExecuteSimpan, () => IsPasswordMatch);
            IsPasswordMatch = true; // Default awal biar tidak langsung merah sebelum diketik
        }

        private void ValidasiPassword()
        {
            // Jika kosong, anggap belum match agar tombol tidak bisa diklik
            if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                IsPasswordMatch = false;
            }
            else
            {
                IsPasswordMatch = (Password == ConfirmPassword);
            }

            // Beritahu RelayCommand untuk mengecek ulang kondisi CanExecute (aktif/tidaknya tombol)
            ((RelayCommand)SimpanPasswordCommand).NotifyCanExecuteChanged();
        }

        private async void ExecuteSimpan()
        {
            try
            {
                // 1. Simpan ke Database (Sesuaikan dengan arsitektur Repository/Service Anda)
                SelectedUser.Password = Password;
                SelectedUser.Status = 1;

                int isSuccess = await service.SaveUserAsync(SelectedUser);
                AppSession.CurrentUser = SelectedUser;


                if (isSuccess > 0)
                {
                    
                    MessageBox.Show("Password berhasil diperbarui!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 2. Buka Window Utama (MainWindow)
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    // 3. Tutup Window Ubah Password saat ini
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.DataContext == this)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan ke database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region INotifyPropertyChanged Helper
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
