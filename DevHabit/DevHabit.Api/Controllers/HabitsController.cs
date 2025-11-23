using System.Linq.Expressions;
using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Habits;
using DevHabit.Api.Dtos.Tags;
using DevHabit.Api.Entities;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
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
    public async Task<ActionResult<HabitDto>> GetHabits([FromQuery] HabitsQueryParameters query)
    {
        Expression<Func<Habit, object>> orderBy = query.Sort switch
        {
            "name" => h => h.Name,
            "description" => h => h.Description ?? string.Empty,
            "type" => h => h.Type,
            _ => h => h.Name
        };

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var habits = await _context.Habits
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search.ToLower()) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search.ToLower()))
            .Where(h => query.Type == null || h.Type.Equals(query.Type))
            .Where(h => query.Status == null || h.Status.Equals(query.Status))
            .OrderBy(orderBy)
            .Include(h => h.Tags)
            .Select(h => h.ToDto())
            .ToListAsync();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

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
            .Select(h => h.ToDto())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        //var validationResult = await validator.ValidateAsync(createHabitDto);

        //if (!validationResult.IsValid)
        //{
        //    var problem = problemDetailsFactory.CreateProblemDetails(
        //        HttpContext,
        //        StatusCodes.Status400BadRequest);

        //    problem.Extensions.Add("errors", validationResult.ToDictionary());

        //    return BadRequest(problem);
        //}

        var habit = createHabitDto.ToEntity();

        _context.Habits.Add(habit);

        await _context.SaveChangesAsync();

        var habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        var habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        _context.Habits.Remove(habit);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
