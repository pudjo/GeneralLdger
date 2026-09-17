using Accounting.DTO;
using Accounting.Services.Accounts;

using Accounting.Services.GeneralLedgerService;
using Accounting.Views.Account;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.Akun
{
    public class DetailAkunViewModel : BaseViewModel // Asumsi Bapak punya BaseViewModel untuk INotifyPropertyChanged
    {
        private AccountDTO _currentAkun;
        private bool _isIdEditable;
        private AccountService accountservice;
        public AccountDTO CurrentAkun
        {
            get => _currentAkun;
            set { _currentAkun = value; OnPropertyChanged(); }
        }

        public bool IsIdEditable
        {
            get => _isIdEditable;
            set { _isIdEditable = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SearchParentCommand { get; }
        private string _saveButtonText;
        public string SaveButtonText
        {
            get => _saveButtonText;
            set { _saveButtonText = value; OnPropertyChanged(); }
        }
        public DetailAkunViewModel( AccountDTO data, bool isNew = false)
        {
            accountservice =  App.ServiceProvider.GetRequiredService<AccountService>();  ;
            CurrentAkun = data;
            if (isNew== false)
            {
                SaveButtonText = "Edit";
            }
            else
            {
                SaveButtonText = "Simpan";
            }
            //IsIdEditable = isNew; // ID hanya bisa diisi jika membuat akun baru
                                  // Tambahkan '_ =>' untuk mengabaikan parameter object dari RelayCommand
            SaveCommand = new RelayCommand(SaveAction);
            DeleteCommand = new RelayCommand(DeleteAction);
            EditCommand = new RelayCommand(EditAction);
            SearchParentCommand = new RelayCommand(FindParentAction);

        }

        private async void SaveAction()
        {
            try
            {
                // 1. Wajib pakai await agar proses penyimpanan selesai dulu
                int result = await accountservice.SaveAccountAsync(CurrentAkun);

                if (result > 0)
                {
                    MessageBox.Show("Akun berhasil disimpan!", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 2. Tutup window input secara otomatis setelah berhasil
                    var currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                    if (currentWindow != null)
                    {
                        currentWindow.DialogResult = true; // Ini akan membuat ShowDialog() mereturn true di form pemanggil
                        currentWindow.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Gagal menyimpan akun.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void DeleteAction()
        {
            try
            {
                int countChildren = await accountservice.GetChildrenCount(CurrentAkun.Id);
                if (countChildren > 0)
                {
                    MessageBox.Show("Akun ini memiliki anak, tidak bisa dihapus.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Apakah Bapak yakin ingin menghapus aset ini?", "Konfirmasi", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await accountservice.DeleteAccountAsync(CurrentAkun.Id);
                        MessageBox.Show("Akun berhasil dihapus!", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);

                        // 2. Tutup window input secara otomatis setelah berhasil
                        var currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                        if (currentWindow != null)
                        {
                            currentWindow.DialogResult = true; // Ini akan membuat ShowDialog() mereturn true di form pemanggil
                            currentWindow.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return;
        }

        private async void EditAction()
        {
            try
            {
                await accountservice.SaveAccountAsync(CurrentAkun);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void FindParentAction()
        {
            // Cek apakah parameter yang dikirim adalah baris detail
            
                var winPilihAkun = new AkunSelectionWindow(true);

                // Atur Owner agar muncul di tengah form input (opsional tapi bagus)
                winPilihAkun.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

                if (winPilihAkun.ShowDialog() == true)
                {
                    var akunDipilih = winPilihAkun.AkunTerpilih;

                    if (akunDipilih != null)
                    {
                    CurrentAkun.IdParent = akunDipilih.Id;
                    CurrentAkun.ParentName = akunDipilih.Name;
                    // Masukkan data ke baris yang diklik
              
                    }
                }

            }
        }
}
