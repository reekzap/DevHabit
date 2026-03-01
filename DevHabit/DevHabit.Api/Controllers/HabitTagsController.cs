using DevHabit.Api.Database;
using DevHabit.Api.Dtos.HabitTags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[ApiController]
[Route("api/habits/{habitId}/tags")]
public class HabitTagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HabitTagsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public static readonly string Name = nameof(HabitTagsController).Replace("Controller", string.Empty);

    [HttpPut]
    public async Task<ActionResult> UpsertHabitTags(string habitId, UpsertHabitTagsDto upsertHabitTagsDto)
    {
        var habit = await _context.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);

        if (habit == null)
        {
            return NotFound();
        }

        // This is used to track which tags are already associated with the habit
        var currentTagIds = habit.HabitTags
            .Select(ht => ht.TagId)
            .ToHashSet();

        // Validate that all provided TagIds exist in the database
        var newTagIds = await _context.Tags
            .Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        // Check if all TagIds were found
        if (newTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("One or more TagIds are invalid.");
        }

        // This is to remove tags that are not in the new list
        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));

        // This is to add new tags that are not already associated with the habit
        var tagsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();

        habit.HabitTags.AddRange(
            tagsToAdd.Select(tagId => new HabitTag
            {
                HabitId = habitId,
                TagId = tagId,
                CreatedAtUtc = DateTime.UtcNow
            })
        );

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        var habitTag = await _context.HabitTags
            .FirstOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);

        if (habitTag == null)
        {
            return NotFound();
        }

        _context.HabitTags.Remove(habitTag);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
