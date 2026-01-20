using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Dtos.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

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
    private readonly JwtAuthSettings _jwtAuthSettings;

    public AuthController(UserManager<IdentityUser> userManager, ApplicationIdentityDbContext identityDbContext,
        ApplicationDbContext applicationDbContext, TokenProvider tokenProvider, IOptions<JwtAuthSettings> jwtAuthSettings)
    {
        _userManager = userManager;
        _identityDbContext = identityDbContext;
        _applicationDbContext = applicationDbContext;
        _tokenProvider = tokenProvider;
        _jwtAuthSettings = jwtAuthSettings.Value;
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

        var tokenRequest = new TokenRequestDto(identityUser.Id, identityUser.Email);
        var accessTokens = _tokenProvider.Create(tokenRequest);

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthSettings.RefreshTokenExpirationInDays)
        };

        _identityDbContext.RefreshTokens.Add(refreshToken);

        await _identityDbContext.SaveChangesAsync();

        // Note: Commit the transaction to both Identity and Application databases.
        await transaction.CommitAsync();

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

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthSettings.RefreshTokenExpirationInDays)
        };

        _identityDbContext.RefreshTokens.Add(refreshToken);

        await _identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto refreshTokenDto)
    {
        var refreshToken = await _identityDbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

        if (refreshToken == null)
        {
            return Unauthorized();
        }

        if (refreshToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        var tokenRequest = new TokenRequestDto(refreshToken.User.Id, refreshToken.User.Email!);
        var accessTokens = _tokenProvider.Create(tokenRequest);

        refreshToken.Token = accessTokens.RefreshToken;
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthSettings.RefreshTokenExpirationInDays);

        await _identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }
}
