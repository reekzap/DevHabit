using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevHabit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationIdentityDbContext _identityDbContext;
    private readonly ApplicationDbContext _applicationDbContext;

    public AuthController(UserManager<IdentityUser> userManager, ApplicationIdentityDbContext identityDbContext, ApplicationDbContext applicationDbContext)
    {
        _userManager = userManager;
        _identityDbContext = identityDbContext;
        _applicationDbContext = applicationDbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
    {
        // Note: This transaction purpose is to ensure that both Identity and Application databases are in sync.
        using var transaction = await _identityDbContext.Database.BeginTransactionAsync();
        _applicationDbContext.Database.SetDbConnection(_identityDbContext.Database.GetDbConnection());
        await _applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        // End Note

        var identityUser = new IdentityUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.Email
        };

        var result = await _userManager.CreateAsync(identityUser, registerUserDto.Password);

        if (!result.Succeeded)
        {
            var extensions = new Dictionary<string, object?>()
            {
                {
                    "errors",
                    result.Errors.ToDictionary(e => e.Code, e => e.Description)
                }
            };

            return Problem(
                detail: "Unable to regiter user, please try again.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extensions);
        }

        var user = registerUserDto.ToEntity();
        user.IdentityId = identityUser.Id;

        _applicationDbContext.Users.Add(user);
        await _applicationDbContext.SaveChangesAsync();

        // Note: Commit the transaction to both Identity and Application databases.
        await transaction.CommitAsync();

        return Ok(user.Id);
    }

}
