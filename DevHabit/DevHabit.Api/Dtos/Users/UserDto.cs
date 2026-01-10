namespace DevHabit.Api.Dtos.Users;

public sealed record UserDto
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
