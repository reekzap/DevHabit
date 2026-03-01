namespace DevHabit.Api.Dtos.Entries;

public sealed record UpdateEntryDto
{
    public required int Value { get; init; }
    public string? Notes { get; init; }
    public required DateOnly Date { get; init; }
}
