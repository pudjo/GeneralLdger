using Accounting.DTO;
using Accounting.Services.Administration;
using CommunityToolkit.Mvvm.Input;

using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.UserVM
{
    internal class UserDetailViewModel: BaseViewModel
    {
        private readonly UserService _userService;
        private readonly Window _currentWindow;


        private string _saveButtonText;
        public string SaveButtonText
        {
            get => _saveButtonText;
            set { _saveButtonText = value; OnPropertyChanged(); }
        }

        public UserDTO SelectedUser { get; set; }
        public string WindowTitle { get; set; }
        public bool IsAddMode { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public UserDetailViewModel(UserService userService, UserDTO user, Window window)
        {
            _userService = userService;
            _currentWindow = window;
            


            if (user == null)
            {
                // NULL for the user value, it's meant add user
                SelectedUser = new UserDTO { Status = 1 }; // Default langsung Aktif (1)
                IsAddMode = true;
                WindowTitle = "Tambah User Baru";
                SaveButtonText = "Simpan";
            }
            else
            {
                // if  the user is not null, it's mean show the user detail. User can edit
                // So display the detail
                SelectedUser = new UserDTO
                {
                    Id = user.Id,
                    UserID = user.UserID,
                    Name = user.Name,
                    Email = user.Email,
                    Status = user.Status,
                    HandPhoneNumber = user.HandPhoneNumber,
                    Password = user.Password
                };
                SaveButtonText = "Ubah";
                IsAddMode = false;
                WindowTitle = $"Detail User - {user.Name}";
            }
            DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync);
            SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync);
        }

        private async Task ExecuteSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedUser.UserID) || string.IsNullOrWhiteSpace(SelectedUser.Name))
            {
                MessageBox.Show("Kolom User ID dan Nama wajib diisi!", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedUser.Id == 0)
            {
                var allUser = await _userService.GetAlUserDTO();

                var probablyUserExist = allUser.FirstOrDefault(x => x.UserID == SelectedUser.UserID);
                if (probablyUserExist != null)
                {
                    MessageBox.Show("User ID sudah ada", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            int hasil = await _userService.SaveUserAsync(SelectedUser);
            if (hasil > 0)
            {
                // Memberitahu form induk bahwa penyimpanan sukses
                _currentWindow.DialogResult = true;
                _currentWindow.Close();
            }
            else
            {
            
            }
        }
        

        private async Task ExecuteDeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedUser.UserID) || string.IsNullOrWhiteSpace(SelectedUser.Name))
            {
                MessageBox.Show("User ID tidak ada ", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool  hasil = await _userService.DeleteUserAsync(SelectedUser);
            if (hasil  == true  )
            {
                MessageBox.Show("User berhasil dihapus!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                // Memberitahu form induk bahwa penyimpanan sukses
                
            }
            else
            {
                MessageBox.Show("User gagal dihapus!", "Gagal", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            _currentWindow.DialogResult = true;
            _currentWindow.Close();
        }
    }
}