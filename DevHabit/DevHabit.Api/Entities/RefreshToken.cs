using Microsoft.AspNetCore.Identity;

namespace DevHabit.Api.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }

    public IdentityUser User { get; set; } = null!;
}
