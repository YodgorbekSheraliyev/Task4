using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AuthController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if(user is null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return BadRequest(new { Message = "Invalid email or password" });
            }
            if (user.Status == UserStatus.Blocked)
                return BadRequest(new { Message = "User is blocked" });

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _authService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if(await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(new { Message = "Email already exists" });
            }

            User user = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                LastLogin = DateTime.UtcNow,
                Status = UserStatus.Active
            };

            await _context.AddAsync(user);
                        await _context.SaveChangesAsync();
            
            var token = _authService.GenerateToken(user);
            return Ok(new { Message = "User registered successfully", Token = token });
        }
    }
}
