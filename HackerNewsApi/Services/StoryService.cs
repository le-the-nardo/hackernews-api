using HackerNewsApi.Clients;
using HackerNewsApi.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsApi.Services;

public class StoryService
{
    private readonly IHackerNewsClient _client;
    private readonly IMemoryCache _cache;
    private const int MaxConcurrentRequests = 10;
    private static readonly SemaphoreSlim _semaphore = 
        new SemaphoreSlim(MaxConcurrentRequests);

    public StoryService(IHackerNewsClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<List<BestStoryDto>> GetBestStoriesAsync(int n)
    {
        var ids = await _cache.GetOrCreateAsync("best_story_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _client.GetBestStoryIdsAsync();
        });

        var selectedIds = ids.Take(n);

        var tasks = selectedIds.Select(async id =>
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _cache.GetOrCreateAsync($"story_{id}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await _client.GetStoryByIdAsync(id);
                });
            }
            finally
            {
                _semaphore.Release();
            }
        });

        var stories = await Task.WhenAll(tasks);

        return stories
            .Where(s => s != null) 
            .OrderByDescending(s => s.Score)
            .Select(s => new BestStoryDto
            {
                Title = s.Title,
                Uri = s.Url,
                PostedBy = s.By,
                Time = DateTimeOffset.FromUnixTimeSeconds(s.Time),
                Score = s.Score,
                CommentCount = s.Descendants
            })
            .ToList();
    }
}