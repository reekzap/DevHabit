using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Habits;

public sealed record HabitsCollectionDto
{
    public List<HabitDto> Data { get; init; } = [];
}

public sealed record HabitDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public HabitType Type { get; init; }
    public FrequencyDto Frequency { get; init; } = null!;
    public TargetDto Target { get; init; } = null!;
    public HabitStatus Status { get; init; }
    public bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MileStoneDto? MileStone { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAtUtc { get; init; }
}

public sealed record FrequencyDto
{
    public FrequencyTpe Type { get; init; }
    public int TimePerPeriod { get; init; }
}

public sealed record TargetDto
{
    public int Value { get; init; }
    public string Unit { get; init; } = string.Empty;
}

public sealed record MileStoneDto
{
    public int Target { get; init; }
    public int Current { get; init; }
}
