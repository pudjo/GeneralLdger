
namespace Accounting.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Status { get; set; } = 0;
        public string HandPhoneNumber { get; set; }
        public string Password { get; set; }
        public List<int> Roles{ get; set; }
    }
}
