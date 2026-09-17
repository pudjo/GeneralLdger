using Accounting.Applications;
using Accounting.Domain.Entities;

using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows.Media.Imaging;

namespace Accounting.ViewModels
{
    internal partial class CompanySettingViewModel : BaseViewModel
    {
        public string Title => throw new NotImplementedException();

        private CompanySetting _currentCompany;
        private BitmapImage _companyLogoImage;

        public CompanySettingViewModel()
        {
            CompanyLogic companyLogic = new CompanyLogic();
            CurrentCompany = companyLogic.GetCompany();
            LoadImage();
        }

        public CompanySetting CurrentCompany
        {
            get => _currentCompany;
            set { _currentCompany = value; OnPropertyChanged(); }
        }

        public BitmapImage CompanyLogoImage
        {
            get => _companyLogoImage;
            set { _companyLogoImage = value; OnPropertyChanged(); }
        }

        [RelayCommand]
        private void Save()
        {
            // Logika Dapper untuk Update tabel Company
            // _repository.Update(CurrentCompany);
        }

        [RelayCommand]
        private void ChangeLogo()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                _currentCompany.Logo = File.ReadAllBytes(openFileDialog.FileName);
                LoadImage(); // Refresh tampilan logo
            }
        }

        private void LoadImage()
        {
            if (CurrentCompany?.Logo == null || CurrentCompany.Logo.Length == 0) return;

            using (var ms = new MemoryStream(CurrentCompany.Logo))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();

                CompanyLogoImage = image;


            }
        }
    }
}