namespace testapp.Domain.Models
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
