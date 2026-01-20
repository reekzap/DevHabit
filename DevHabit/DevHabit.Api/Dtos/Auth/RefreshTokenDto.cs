namespace DevHabit.Api.Dtos.Auth;

public sealed record RefreshTokenDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
