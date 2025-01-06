using Microsoft.AspNetCore.Mvc;
using ZePost.ApiService.Infrastructure.Services;
using ZePost.ApiService.Infrastructure.Services.YTServ;
using ZePost.ApiService.Infrastructure.Services.YTServ.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<YouTubeSettings>(
    builder.Configuration.GetSection("YouTube"));
builder.Services.AddScoped<IYouTubeService, YTService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/youtube/{channelId}", async (string channelId, IYouTubeService youTubeService) =>
{
    try
    {
        var channelData = await youTubeService.GetChannelDataAsync(channelId);
        return Results.Ok(channelData);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("GetYouTubeChannelData")
.WithOpenApi();

app.MapGet("/youtube/{channelId}/videos", async (
    string channelId, 
    [FromQuery] string? pageToken,
    [FromQuery] int? maxResults,
    IYouTubeService youTubeService) =>
{
    try
    {
        var videos = await youTubeService.GetChannelVideosAsync(
            channelId,
            pageToken,
            maxResults ?? 50);
        return Results.Ok(videos);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("GetYouTubeChannelVideos")
.WithOpenApi();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
