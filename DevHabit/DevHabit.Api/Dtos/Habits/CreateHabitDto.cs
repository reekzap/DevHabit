using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Habits;

public sealed record CreateHabitDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public HabitType Type { get; init; }
    public FrequencyDto Frequency { get; init; } = null!;
    public TargetDto Target { get; init; } = null!;
    public DateOnly? EndDate { get; init; }
    public MileStoneDto? MileStone { get; init; }
}


