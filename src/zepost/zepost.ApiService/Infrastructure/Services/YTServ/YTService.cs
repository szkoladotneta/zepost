using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using ZePost.ApiService.Infrastructure.Services.YTServ.Models;

namespace ZePost.ApiService.Infrastructure.Services.YTServ;

public interface IYouTubeService
{
    Task<ChannelData> GetChannelDataAsync(string channelId, CancellationToken cancellationToken = default);
    Task<PagedVideoResult> GetChannelVideosAsync(string channelId, string? pageToken = null, int maxResults = 50, CancellationToken cancellationToken = default);
}

public class YTService : IYouTubeService
{
    private readonly YouTubeService _youTubeService;

    public YTService(IOptions<YouTubeSettings> settings)
    {
        _youTubeService = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = settings.Value.ApiKey,
            ApplicationName = "ZePost API"
        });
    }

    public async Task<ChannelData> GetChannelDataAsync(string channelId, CancellationToken cancellationToken = default)
    {
        var channelRequest = _youTubeService.Channels.List("snippet,statistics,contentDetails");
        channelRequest.Id = channelId;

        var channelResponse = await channelRequest.ExecuteAsync(cancellationToken);
        var channel = channelResponse.Items.FirstOrDefault();

        if (channel == null)
            throw new KeyNotFoundException($"Channel with ID {channelId} not found");

        return new ChannelData
        {
            Title = channel.Snippet.Title,
            Description = channel.Snippet.Description,
            SubscriberCount = channel.Statistics.SubscriberCount ?? 0,
            ViewCount = channel.Statistics.ViewCount ?? 0,
            VideoCount = channel.Statistics.VideoCount ?? 0,
            ThumbnailUrl = channel.Snippet.Thumbnails.High.Url,
            PublishedAt = channel.Snippet.PublishedAt ?? DateTime.MinValue
        };
    }

    public async Task<PagedVideoResult> GetChannelVideosAsync(string channelId, string? pageToken = null, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel ID cannot be empty", nameof(channelId));
        }

        if (maxResults < 1 || maxResults > 50)
        {
            maxResults = 50;
        }

        try {
        var searchRequest = _youTubeService.Search.List("snippet");
        searchRequest.ChannelId = channelId;
        searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
        searchRequest.MaxResults = maxResults;
        searchRequest.Type = "video";
        searchRequest.PageToken = pageToken;

        var searchResponse = await searchRequest.ExecuteAsync(cancellationToken);
        var videoIds = string.Join(",", searchResponse.Items.Select(i => i.Id.VideoId));

        var videos = new List<VideoData>();
        if (!string.IsNullOrEmpty(videoIds))
        {
            var videosRequest = _youTubeService.Videos.List("snippet,statistics,contentDetails");
            videosRequest.Id = videoIds;

            var videosResponse = await videosRequest.ExecuteAsync(cancellationToken);

            videos.AddRange(videosResponse.Items.Select(video => new VideoData
            {
                Id = video.Id,
                Title = video.Snippet.Title,
                Description = video.Snippet.Description,
                PublishedAt = video.Snippet.PublishedAt ?? DateTime.MinValue,
                ThumbnailUrl = video.Snippet.Thumbnails.High?.Url ?? "",
                ViewCount = video.Statistics.ViewCount ?? 0,
                LikeCount = video.Statistics.LikeCount ?? 0,
                CommentCount = video.Statistics.CommentCount ?? 0,
                Duration = ParseDuration(video.ContentDetails.Duration),
                Tags = video.Snippet.Tags?.ToList() ?? new List<string>()
            }));
        }

        return new PagedVideoResult
        {
            Videos = videos,
            NextPageToken = searchResponse.NextPageToken,
            PrevPageToken = searchResponse.PrevPageToken,
                TotalResults = searchResponse.PageInfo?.TotalResults ?? 0
            };
        }
        catch (Google.GoogleApiException ex)
        {
            throw new InvalidOperationException($"YouTube API error: {ex.Message}", ex);
        }
    }

    private TimeSpan ParseDuration(string duration)
    {
        try
        {
            return System.Xml.XmlConvert.ToTimeSpan(duration);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}
