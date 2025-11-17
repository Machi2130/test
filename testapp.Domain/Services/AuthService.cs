using testapp.Domain.Interfaces;
using testapp.Domain.Models;
using testapp.Domain.Results;
using testapp.Domain.Utils;
using testapp.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace testapp.Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _userRepo;
        private readonly IRolePermissionRepo _rolePermissionRepo;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepo userRepo, IRolePermissionRepo rolePermissionRepo, ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _logger = logger;
        }

       
        public async Task<AuthResult> AuthenticateAsync(LoginRequestDto request)
        {
            bool success = false;
            string? error = null;
            LoginResponseDto? loginDetails = null;

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                error = "Username and password are required.";
            }
            else
            {
                var user = await _userRepo.GetByUsernameAsync(request.Username);

                if (user == null)
                {
                    error = "Invalid credentials.";
                }
                else if (!user.IsActive)
                {
                    error = "User is inactive. Please contact Administrator.";
                }
                else
                {
                    try
                    {
                        var passwordMatches = PasswordHasher.Verify(
                            request.Password,
                            user.PasswordHash,
                            user.PasswordSalt
                        );

                        if (!passwordMatches)
                        {
                            error = "Invalid credentials.";
                        }
                        else
                        {
                            var roles = (await _rolePermissionRepo.GetRolesByUserIdAsync(user.UserId)).ToArray();
                            var permissions = (await _rolePermissionRepo.GetPermissionsByUserIdAsync(user.UserId)).ToArray();

                            loginDetails = new LoginResponseDto
                            {
                                UserId = user.UserId,
                                Username = user.Username,
                                Roles = roles,
                                Permissions = permissions
                            };

                            success = true;
                        }
                    }
                    catch (FormatException)
                    {
                        error = "Stored password hash or salt is invalid. Please contact Administrator.";
                    }
                }
            }
            // Logging
            if (success)
                _logger.LogInformation("User {username} logged in successfully.", request.Username);
            else
                _logger.LogWarning("Login failed for {username}. Reason: {error}", request.Username, error);

            return new AuthResult
            {
                Success = success,
                Error = error,
                Data = loginDetails
            };
        }


        public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequestDto request)
        {
            // check if user already exists
            var existing = await _userRepo.GetByUsernameAsync(request.Username);
            if (existing != null) return (false, "Username already exists.");

            // hash password
            var (hash, salt) = PasswordHasher.HashPassword(request.Password);

            // create user
            var user = new testapp.DAL.Models.User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true
            };
            var userId = await _userRepo.CreateUserAsync(user);

            // assign role
            await _userRepo.AssignRoleAsync(userId, request.RoleId);

            return (true, null);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();

            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive
            });
        }
    }
}
