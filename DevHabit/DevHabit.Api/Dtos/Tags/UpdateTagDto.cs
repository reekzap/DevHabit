namespace DevHabit.Api.Dtos.Tags;

public sealed record UpdateTagDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
