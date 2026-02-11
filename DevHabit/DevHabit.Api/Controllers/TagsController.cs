using DevHabit.Api.Database;
using DevHabit.Api.Dtos.Habits;
using DevHabit.Api.Dtos.Tags;
using DevHabit.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserContext _userContext;

    public TagsController(ApplicationDbContext context, UserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }


    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var tags = await _context.Tags
            .Where(c => c.UserId == userId)
            .Select(c => c.ToDto())
            .ToListAsync();

        var habitsCollectionDto = new TagsCollectionDto
        {
            Data = tags
        };

        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var tag = await _context.Tags
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => c.ToDto())
            .FirstOrDefaultAsync();

        if (tag is null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto, IValidator<CreateTagDto> validator, ProblemDetailsFactory problemDetailsFactory)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var validationResult = await validator.ValidateAsync(createTagDto);

        if (!validationResult.IsValid)
        {
            var problem = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest);

            problem.Extensions.Add("errors", validationResult.ToDictionary());

            return BadRequest(problem);
        }

        var tag = createTagDto.ToEntity(userId);

        if (await _context.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Conflict($"The tag '{tag.Name}' already exists");
        }

        _context.Tags.Add(tag);

        await _context.SaveChangesAsync();

        var tagDto = tag.ToDto();

        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto updateTagDto)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var tag = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (tag is null)
        {
            return NotFound();
        }

        tag.UpdateFromDto(updateTagDto);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        var userId = await _userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var tag = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (tag is null)
        {
            return NotFound();
        }

        _context.Tags.Remove(tag);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
