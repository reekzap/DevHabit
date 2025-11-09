using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Habits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HabitsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HabitsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<HabitDto>> GetHabits()
    {
        var habits = await _context.Habits
            .Select(h => new HabitDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Type = h.Type,
                Frequency = new FrequencyDto
                {
                    Type = h.Frequency.Type,
                    TimePerPeriod = h.Frequency.TimePerPeriod
                },
                Target = new TargetDto
                {
                    Value = h.Target.Value,
                    Unit = h.Target.Unit
                },
                Status = h.Status,
                IsArchived = h.IsArchived,
                EndDate = h.EndDate,
                MileStone = h.MileStone != null ? new MileStoneDto
                {
                    Target = h.MileStone.Target,
                    Current = h.MileStone.Current
                } : null,
                CreatedAtUtc = h.CreatedAtUtc,
                UpdatedAtUtc = h.UpdatedAtUtc,
                LastCompletedAtUtc = h.LastCompletedAtUtc
            })
            .ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto
        {
            Data = habits
        };

        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        var habit = await _context.Habits
            .Where(h => h.Id == id)
            .Select(h => new HabitDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Type = h.Type,
                Frequency = new FrequencyDto
                {
                    Type = h.Frequency.Type,
                    TimePerPeriod = h.Frequency.TimePerPeriod
                },
                Target = new TargetDto
                {
                    Value = h.Target.Value,
                    Unit = h.Target.Unit
                },
                Status = h.Status,
                IsArchived = h.IsArchived,
                EndDate = h.EndDate,
                MileStone = h.MileStone != null ? new MileStoneDto
                {
                    Target = h.MileStone.Target,
                    Current = h.MileStone.Current
                } : null,
                CreatedAtUtc = h.CreatedAtUtc,
                UpdatedAtUtc = h.UpdatedAtUtc,
                LastCompletedAtUtc = h.LastCompletedAtUtc
            })
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }
}
