
namespace Accounting.DTO
{
   public  class LoginDTO:BaseViewModel
    {
        private string _userID;
        public string UserID
        {
            set
           {
                _userID = value;
                OnPropertyChanged(nameof(UserID));
            }
            get => _userID;

        }
        private string _password;
        public string Password
        {
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
            get => _password;

        }
        private string _year;
        public string Year
        {
            set
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
            }
            get => _year;

        }



    }
}
