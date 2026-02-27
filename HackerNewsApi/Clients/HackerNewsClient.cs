using System.Text.Json;
using HackerNewsApi.Models;

namespace HackerNewsApi.Clients;

public class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;

    public HackerNewsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<int>> GetBestStoryIdsAsync()
    {
        var response = await _httpClient.GetAsync("v0/beststories.json");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<int>>(content);
    }

    public async Task<HackerNewsStory> GetStoryByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"v0/item/{id}.json");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<HackerNewsStory>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}