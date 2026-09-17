using Accounting.Domain;
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.Services.Administration;
using Accounting.Views.UserView;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.UserVM
{
    public  class LoginViewModel:BaseViewModel 
    {
        //Fields
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        public event Action OnLoginSuccess;



        public bool IsViewVisible
        {
            get => _isViewVisible;
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }
        private int _selectedYear = 2026; // Nilai default (bisa disesuaikan dengan tahun berjalan)
        public int  SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
            }
        }

        private LoginDTO _logindto;
        public LoginDTO LoginDto
        {
            get => _logindto;
            set { _logindto = value; OnPropertyChanged(); }
        }

        private UserService userservice;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }

            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        //-> Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        //Constructor
        public LoginViewModel(UserService _userservice)
        {
            this.userservice = _userservice;
            LoginDto = new LoginDTO();

            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            //RecoverPasswordCommand = new RelayCommand(ExecuteLoginCommand);  //ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
        }

        private async void ExecuteLoginCommand()
        {

            AppSession.SelectedYear = SelectedYear;
            UserDTO  isValidUser = await userservice.UserLogin(LoginDto);
            
            if (isValidUser== null)
            {
                
                return;
            }
            UserDTO loggedUser = await userservice.GetUserDTOByID(isValidUser.Id);
            if (loggedUser.Status == 0)
            {
                MessageBox.Show("Ini adalah login pertama Anda atau password Anda di-reset. Silakan buat password baru.",
                            "Perhatian", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // 1. Buat ViewModel sambil melempar data user hasil query login
                var targetViewModel = new CreatePasswordViewModel(isValidUser);

                // 2. Buat Window Ubah Password
                var createPassWindow = new CreatePasswordView();

                // 3. Pasangkan ViewModel ke DataContext Jendela baru tersebut
                createPassWindow.DataContext = targetViewModel;

                // 4. Tampilkan form
                createPassWindow.ShowDialog();
                // Panggil form Ubah Password dari Service Provider


            } 

            
            
            AppSession.CurrentUser = loggedUser;
            // Picu event sukses!
            OnLoginSuccess?.Invoke();
            //}
            //else
            //{
                // Tampilkan pesan error (bisa lewat properti status)
            //}


        }

    }
}
