namespace DevHabit.Api.Entities;

public sealed class Users
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Store the IdentityId from the Identity provider.
    /// This cound be any identity provider like Azure AD B2C, Auth0, Firebase Auth, etc.
    /// </summary>
    public string IdetityId { get; set; } = string.Empty;
}
