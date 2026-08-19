using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        [HttpPost("block")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && user.Id.ToString() == currentUserId)
            {
                return BadRequest(new { message = "You cannot block your own account." });
            }

            user.Status = UserStatus.Blocked;
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("unblock")]
        public async Task<IActionResult> UnblockUser([FromBody] UnblockUserDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && user.Id.ToString() == currentUserId)
            {
                return BadRequest(new { message = "You cannot unblock your own account." });
            }

            user.Status = UserStatus.Active;
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && user.Id.ToString() == currentUserId)
            {
                return BadRequest(new { message = "You cannot delete your own account." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }
    }
}
