namespace DevHabit.Api.Dtos.HabitTags;

public sealed record UpsertHabitTagsDto
{
    public List<string> TagIds { get; init; } = [];
}
