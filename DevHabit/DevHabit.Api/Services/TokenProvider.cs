using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DevHabit.Api.Dtos.Auth;
using DevHabit.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DevHabit.Api.Services;

public sealed class TokenProvider
{
    private readonly JwtAuthSettings _settings;

    public TokenProvider(IOptions<JwtAuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public AccesTokensDto Create(TokenRequestDto tokenRequest)
    {
        return new AccesTokensDto(GenerateAccessToken(tokenRequest), GenerateRefreshToken());
    }

    public string GenerateAccessToken(TokenRequestDto tokenRequest)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, tokenRequest.UserId),
            new Claim(JwtRegisteredClaimNames.Email, tokenRequest.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience
        };

        var handler = new JsonWebTokenHandler();

        var accessToken = handler.CreateToken(tokenDescriptor);

        return accessToken;
    }

    public static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(randomBytes);
    }
}
