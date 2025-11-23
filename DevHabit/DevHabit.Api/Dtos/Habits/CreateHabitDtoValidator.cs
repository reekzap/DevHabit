using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.Dtos.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits =
    [
        "times",
        "minutes",
        "hours",
        "pages",
        "kilometers",
        "words",
        "minutes"
    ];

    private static readonly string[] AllowedUnitsForBinaryHabits =
    [
        "sessions",
        "tasks"
    ];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Type)
            .IsInEnum();

        // Frequency validation
        RuleFor(x => x.Frequency.Type)
            .IsInEnum();

        RuleFor(x => x.Frequency.TimePerPeriod)
            .GreaterThan(0);

        // Target validation
        RuleFor(x => x.Target.Value)
            .GreaterThan(0);

        RuleFor(x => x.Target.Unit)
            .NotEmpty()
            .Must(unit => AllowedUnits.Contains(unit.ToLowerInvariant()))
            .WithMessage($"Unit must be one of: {string.Join(", ", AllowedUnits)}");

        // EndDate validation
        RuleFor(x => x.EndDate)
            .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be in the future.");

        //MileStone validation
        When(x => x.MileStone is not null, () =>
        {
            RuleFor(x => x.MileStone!.Target)
                .GreaterThan(0);
        });

        //Complex rules
        RuleFor(x => x.Target.Unit)
            .Must((dto, unit) => IsTargetUnitCompatibleWithType(dto.Type, unit))
            .WithMessage("Target unit is not compatible with the habit type.");
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        var normalizeUnit = unit.ToLowerInvariant();

        return type switch
        {
            HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizeUnit),
            HabitType.Measurable => AllowedUnits.Contains(normalizeUnit),
            _ => false
        };
    }
}
