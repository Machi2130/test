using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using testapp.DAL.Interfaces;
using testapp.DAL.Models;
using testapp.Domain.Interfaces;
using testapp.Domain.Models;
using testapp.Server.Config;
using UAParser;

namespace testapp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILoginLogRepo _loginLogRepo;

        public AuthController(
            IAuthService authService,
            IOptions<JwtSettings> jwtOptions,
            ILoginLogRepo loginLogRepo)
        {
            _authService = authService;
            _jwtSettings = jwtOptions.Value;
            _loginLogRepo = loginLogRepo;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var (success, error) = await _authService.RegisterAsync(request);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.AuthenticateAsync(request);

            if (!result.Success)
                return Unauthorized(new { message = result.Error });

            var token = GenerateToken(result.Data!);

            var parser = Parser.GetDefault();
            var clientInfo = parser.Parse(Request.Headers["User-Agent"].ToString());

            var uaFamily = clientInfo?.UA?.Family ?? "Unknown";
            var uaMajor = clientInfo?.UA?.Major;
            var browser = string.IsNullOrEmpty(uaMajor)
                ? uaFamily
                : $"{uaFamily} {uaMajor}";

            var device = string.IsNullOrWhiteSpace(clientInfo!.Device.Family) || clientInfo.Device.Family == "Other"
                ? "Desktop"
                : clientInfo.Device.Family;

            var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            var loginTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            var log = new LoginLog
            {
                UserId = result.Data!.UserId,
                IPAddress = GetClientIp(),
                Browser = browser,
                Device = device,
                LoginTime = loginTime,
                LogoutTime = null,
                IsSuccess = true
            };

            await _loginLogRepo.CreateLogAsync(log);

            var response = new LoginResponseDto
            {
                UserId = result.Data!.UserId,
                Username = result.Data.Username,
                Token = token,
                Roles = result.Data.Roles,
                Permissions = result.Data.Permissions
            };

            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _authService.GetAllUsersAsync();

            if (users == null || !users.Any())
                return NotFound ("No users found.");

            return Ok(users);
        }

        private string GetClientIp()
        {
            // Check X-Forwarded-For (used when behind proxy/load balancer)
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Can contain multiple IPs, first is the real client IP
                return forwardedFor.Split(',')[0];
            }

            // Fall back to direct remote IP
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GenerateToken(LoginResponseDto data)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, data.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, data.Username)
            };

            foreach (var r in data.Roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            foreach (var p in data.Permissions)
                claims.Add(new Claim("permission", p));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
