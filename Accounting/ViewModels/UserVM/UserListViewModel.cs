using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.Services.Administration;
using Accounting.Views.UserView;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.UserVM
{
    internal class UserListViewModel:BaseViewModel
    {
        // 1. Deklarasikan field global dengan underscore (_) agar rahasia di dalam kelas ini
        private readonly UserService _userService;

        //public ObservableCollection<UserDTO> DaftarUser { get; set; }
        
        public ICommand ShowDetailCommand { get; }
        private ObservableCollection<UserDTO> _daftarUser = new();
        public ObservableCollection<UserDTO> DaftarUser
        {
            get => _daftarUser;
            set => SetProperty(ref _daftarUser, value);
        }
        // 2. Terima UserService dari ServiceCollection lewat constructor. DI
        public UserListViewModel(UserService userService)
        {
            // Hubungkan parameter ke field global dengan tepat
            _userService = userService;
            ShowDetailCommand = new RelayCommand<UserDTO>(ExecuteOpenDetail);
            _userService.UsersChanged += OnUserChange;
            _ = LoadDaftarUserAsync();
        }

        


        private async void OnUserChange(Object?sender , EventArgs e )
        {
            await LoadDaftarUserAsync();
        }


        private void ExecuteOpenDetail(UserDTO selectedUser)
        {
            // Take wondows owner, 
            Window parentWindow = Application.Current.MainWindow;
            UserDetailWindow detailWindow = new UserDetailWindow { Owner = parentWindow };
            // Pasang ViewModel khusus detail ke dalam Window tersebut
            UserDetailViewModel detailViewModel = new UserDetailViewModel(_userService, selectedUser, detailWindow);
            detailWindow.DataContext = detailViewModel;
            //  Show the Window in dialog mode
             
            bool? isSaved = detailWindow.ShowDialog();
            // Saved? SO refresh the displayed list of user
            if (isSaved == true)
            {
                _ = LoadDaftarUserAsync();
            }
        }
        // Load data
        private async Task LoadDaftarUserAsync()
        {
            try
            {
                
                // Ambil data secara async tanpa memblokir UI thread
                var data = await _userService.GetAlUserDTO();
                DaftarUser = new ObservableCollection<UserDTO>(data);
            }
            catch (Exception ex)
            {
                // Tangani error atau log di sini jika proses load gagal
                Debug.WriteLine($"Gagal memuat data user: {ex.Message}");
            }
        }
    }

}
