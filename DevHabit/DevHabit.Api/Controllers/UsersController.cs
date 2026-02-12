using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserContext _userContext;

    public UsersController(ApplicationDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var userId = await _userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (userId != id)
        {
            return Forbid();
        }

        var user = await _context.Users
            .Where(u => u.Id.Equals(id))
            .Select(u => u.ToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = await _userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Where(u => u.Id.Equals(userId))
            .Select(u => u.ToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
