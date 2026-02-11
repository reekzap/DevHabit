using System.Linq.Dynamic.Core;
using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Common;
using DevHabit.Api.Dtos.Habits;
using DevHabit.Api.Dtos.Tags;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class HabitsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserContext _userContext;

    public HabitsController(ApplicationDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResult<HabitDto>>> GetHabits([FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'");
        }

        var sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var habitsQuery = _context.Habits
            .Where(h => h.UserId == userId)
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search.ToLower()) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search.ToLower()))
            .Where(h => query.Type == null || h.Type.Equals(query.Type))
            .Where(h => query.Status == null || h.Status.Equals(query.Status))
            .ApplySort(query.Sort, sortMappings)
            .Include(h => h.Tags)
            .Select(h => h.ToDto())
            .AsQueryable();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        var paginationResult = await PaginationResult<HabitDto>.CreateAsync(
            habitsQuery,
            query.Page,
            query.PageSize);

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var habit = await _context.Habits
            .Where(h => h.Id == id && h.UserId == userId)
            .Include(h => h.Tags)
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
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

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

        var habit = createHabitDto.ToEntity(userId);

        _context.Habits.Add(habit);

        await _context.SaveChangesAsync();

        var habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

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
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

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
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        _context.Habits.Remove(habit);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
