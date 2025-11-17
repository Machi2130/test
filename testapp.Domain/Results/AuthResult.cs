using testapp.Domain.Models;

namespace testapp.Domain.Results
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public LoginResponseDto? Data { get; set; }
        public static AuthResult Fail(string error) => new() { Success = false, Error = error };
        public static AuthResult Ok(LoginResponseDto data) => new() { Success = true, Data = data };
    }
}