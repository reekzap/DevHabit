using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;

namespace DevHabit.Api.Dtos.Habits;

public static class HabitMapper
{
    public static readonly SortMappingDefinition<HabitDto, Habit> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(HabitDto.Name), nameof(Habit.Name)),
            new SortMapping(nameof(HabitDto.Description), nameof(Habit.Description)),
            new SortMapping(nameof(HabitDto.Type), nameof(Habit.Type)),
            new SortMapping(
                $"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.Type)}",
                $"{nameof(Habit.Frequency)}.{nameof(Frequency.Type)}"),
            new SortMapping(
                $"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.TimePerPeriod)}",
                $"{nameof(Habit.Frequency)}.{nameof(Frequency.TimePerPeriod)}"),
            new SortMapping(
                $"{nameof(HabitDto.Target)}.{nameof(TargetDto.Value)}",
                $"{nameof(Habit.Target)}.{nameof(Target.Value)}"),
            new SortMapping(
                $"{nameof(HabitDto.Target)}.{nameof(TargetDto.Unit)}",
                $"{nameof(Habit.Target)}.{nameof(Target.Unit)}"),
            new SortMapping(nameof(HabitDto.Status), nameof(Habit.Status)),
            new SortMapping(nameof(HabitDto.EndDate), nameof(Habit.EndDate)),
            new SortMapping(nameof(HabitDto.CreatedAtUtc), nameof(Habit.CreatedAtUtc)),
            new SortMapping(nameof(HabitDto.UpdatedAtUtc), nameof(Habit.UpdatedAtUtc)),
            new SortMapping(nameof(HabitDto.LastCompletedAtUtc), nameof(Habit.LastCompletedAtUtc))
        ]
    };

    public static Habit ToEntity(this CreateHabitDto dto, string userId)
    {
        return new Habit
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = dto.Name,
            UserId = userId,
            Description = dto.Description,
            Type = dto.Type,
            Frequency = new Frequency
            {
                Type = dto.Frequency.Type,
                TimePerPeriod = dto.Frequency.TimePerPeriod
            },
            Target = new Target
            {
                Value = dto.Target.Value,
                Unit = dto.Target.Unit
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = dto.EndDate,
            MileStone = dto.MileStone != null ? new MileStone
            {
                Target = dto.MileStone.Target,
                Current = 0 // Initialize current progress to 0
            } : null,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimePerPeriod = habit.Frequency.TimePerPeriod
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            MileStone = habit.MileStone != null ? new MileStoneDto
            {
                Target = habit.MileStone.Target,
                Current = habit.MileStone.Current
            } : null,
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc,
            Tags = habit.Tags
                .Select(t => t.Name)
                .ToList()
        };
    }



    public static void UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;
        habit.EndDate = dto.EndDate;

        habit.Frequency = new Frequency
        {
            Type = dto.Frequency.Type,
            TimePerPeriod = dto.Frequency.TimePerPeriod
        };

        habit.Target = new Target
        {
            Value = dto.Target.Value,
            Unit = dto.Target.Unit
        };

        if (dto.MileStone is not null)
        {
            habit.MileStone ??= new MileStone();
            habit.MileStone.Target = dto.MileStone.Target;
        }

        habit.UpdatedAtUtc = DateTime.UtcNow;
    }
}
