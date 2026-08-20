using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (int.TryParse(userId, out var id))
            {
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    context.Response.StatusCode =
                        StatusCodes.Status401Unauthorized;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "User no longer exists."
                    });

                    return;
                }


                if (user.Status == UserStatus.Blocked)
                {
                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Your account has been blocked."
                    });

                    return;
                }
            }
        }

        await _next(context);
    }
}