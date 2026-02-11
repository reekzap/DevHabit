using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Tags;

internal static class TagMapper
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }

    public static Tag ToEntity(this CreateTagDto dto, string userId)
    {
        return new Tag()
        {
            Id = $"t_{Guid.CreateVersion7()}",
            Name = dto.Name,
            UserId = userId,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}
