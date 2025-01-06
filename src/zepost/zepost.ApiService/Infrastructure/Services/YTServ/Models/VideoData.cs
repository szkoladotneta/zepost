namespace ZePost.ApiService.Infrastructure.Services.YTServ.Models;

public record VideoData
{
    public string Id { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public DateTime PublishedAt { get; init; }
    public string ThumbnailUrl { get; init; } = default!;
    public ulong ViewCount { get; init; }
    public ulong LikeCount { get; init; }
    public ulong CommentCount { get; init; }
    public TimeSpan Duration { get; init; }
    public List<string> Tags { get; init; } = new();
}
