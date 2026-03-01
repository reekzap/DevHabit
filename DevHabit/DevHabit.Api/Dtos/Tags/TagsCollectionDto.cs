using DevHabit.Api.Dtos.Common;

namespace DevHabit.Api.Dtos.Tags;

public sealed record TagsCollectionDto : ICollectionResponse<TagDto>, ILinksResponse
{
    public List<TagDto> Data { get; init; } = [];
    public List<LinkDto> Links { get; set; } = [];
}
