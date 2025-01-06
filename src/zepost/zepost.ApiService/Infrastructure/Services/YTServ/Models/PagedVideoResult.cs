namespace ZePost.ApiService.Infrastructure.Services.YTServ.Models;

public record PagedVideoResult
{
    public IEnumerable<VideoData> Videos { get; init; } = new List<VideoData>();
    public string? NextPageToken { get; init; }
    public string? PrevPageToken { get; init; }
    public long TotalResults { get; init; }
}
