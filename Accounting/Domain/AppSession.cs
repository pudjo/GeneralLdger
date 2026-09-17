using Accounting.DTO;

namespace Accounting.Domain
{
    public static class AppSession
    {
        public static int SelectedYear { get; set; }
        public static UserDTO CurrentUser { get; set; }
        

    }
}
