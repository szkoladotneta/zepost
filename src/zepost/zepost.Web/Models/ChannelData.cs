namespace zepost.Web.Models;

public record ChannelData
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public ulong SubscriberCount { get; init; }
    public ulong ViewCount { get; init; }
    public ulong VideoCount { get; init; }
    public string ThumbnailUrl { get; init; } = default!;
    public DateTime PublishedAt { get; init; }
}