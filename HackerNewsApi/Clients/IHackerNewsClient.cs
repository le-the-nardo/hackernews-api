using HackerNewsApi.Models;

namespace HackerNewsApi.Clients;

public interface IHackerNewsClient
{
    Task<List<int>> GetBestStoryIdsAsync();
    Task<HackerNewsStory?> GetStoryByIdAsync(int id);
}