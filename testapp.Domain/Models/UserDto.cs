namespace testapp.Domain.Models
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        }
}
