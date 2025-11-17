using testapp.Domain.Models;
using testapp.Domain.Results;

namespace testapp.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> AuthenticateAsync(LoginRequestDto request);
        Task<(bool Success, string? Error)> RegisterAsync(RegisterRequestDto request);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
}
