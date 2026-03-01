using DevHabit.Api.Dtos.Common;

namespace DevHabit.Api.Dtos.Users;

public sealed record UserDto : ILinksResponse
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public List<LinkDto> Links { get; set; } = [];
}
