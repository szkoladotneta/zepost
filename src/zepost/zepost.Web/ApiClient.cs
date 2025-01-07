using System.Net.Http.Json;
using zepost.Web.Models;

namespace zepost.Web;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChannelData> GetChannelAsync(
        string channelId, 
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<ChannelData>(
            $"/youtube/{channelId}", 
            cancellationToken);

        return response ?? throw new KeyNotFoundException($"Channel with ID {channelId} not found");
    }

    public async Task<PagedVideoResult> GetVideosAsync(
        string channelId,
        string? pageToken = null,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(pageToken))
            queryParams.Add($"pageToken={pageToken}");
        
        if (maxResults.HasValue)
            queryParams.Add($"maxResults={maxResults}");

        var url = $"/youtube/{channelId}/videos";
        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetFromJsonAsync<PagedVideoResult>(
            url, 
            cancellationToken);

        return response ?? new PagedVideoResult();
    }
}
