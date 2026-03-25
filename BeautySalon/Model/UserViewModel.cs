namespace BeautySalon.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Block { get; set; }
        public bool FirstAuth { get; set; }
    }
}