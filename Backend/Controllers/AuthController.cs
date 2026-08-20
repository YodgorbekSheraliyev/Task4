using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, AuthService authService, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _authService = authService;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user is null ||
                !BCrypt.Net.BCrypt.Verify(
                    loginDto.Password,
                    user.PasswordHash))
            {
                return BadRequest(new
                {
                    Message = "Invalid email or password"
                });
            }

            if (user.Status == UserStatus.Unverified)
            {
                return Unauthorized(new
                {
                    Message = "Please verify your email address before logging in."
                });
            }

            if (user.Status == UserStatus.Blocked)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Message = "Your account has been blocked."
                });
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _authService.GenerateToken(user);

            return Ok(new
            {
                Token = token
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(new
                {
                    Message = "Email already exists"
                });
            }

            var verificationToken = Guid.NewGuid();

            User user = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    registerDto.Password
                ),
                LastLogin = DateTime.UtcNow,
                Status = UserStatus.Unverified,
                EmailVerificationToken = verificationToken.ToString(),
                EmailVerificationTokenExpiresAt =
                    DateTime.UtcNow.AddHours(2)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            var verificationUri = new Uri(
                $"http://{_configuration["ServerIp"]}:{_configuration["ServerPort"]}" +
                $"/api/users/verify-email?token={verificationToken}"
            );

            await _emailService.SendMail(
                verificationUri.ToString(),
                user.Email
            );

            return Ok(new
            {
                Message = "Registration successful. Please verify your email address."
            });
        }
    }
}
