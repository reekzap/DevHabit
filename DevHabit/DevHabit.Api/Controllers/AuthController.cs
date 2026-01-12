using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Dtos.Users;
using DevHabit.Api.Services;
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
    private readonly TokenProvider _tokenProvider;

    public AuthController(UserManager<IdentityUser> userManager, ApplicationIdentityDbContext identityDbContext,
        ApplicationDbContext applicationDbContext, TokenProvider tokenProvider)
    {
        _userManager = userManager;
        _identityDbContext = identityDbContext;
        _applicationDbContext = applicationDbContext;
        _tokenProvider = tokenProvider;
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

        var tokenRequest = new TokenRequestDto(identityUser.Id, identityUser.Email);
        var accessTokens = _tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
    {
        var identityUser = await _userManager.FindByEmailAsync(loginUserDto.Email);

        if (identityUser is null)
        {
            return Unauthorized();
        }
        var passwordValid = await _userManager.CheckPasswordAsync(identityUser, loginUserDto.Password);

        if (!passwordValid)
        {
            return Unauthorized();
        }

        var tokenRequest = new TokenRequestDto(identityUser.Id, identityUser.Email!);
        var accessTokens = _tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }
}
