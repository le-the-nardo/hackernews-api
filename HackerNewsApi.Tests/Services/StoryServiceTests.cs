using HackerNewsApi.Clients;
using HackerNewsApi.Models;
using HackerNewsApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HackerNewsApi.Tests.Services;

public class StoryServiceTests
{
    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturnOnlyNStories()
    {
        var mockClient = new Mock<IHackerNewsClient>();

        mockClient.Setup(c => c.GetBestStoryIdsAsync())
            .ReturnsAsync(new List<int> { 1, 2, 3 });

        mockClient.Setup(c => c.GetStoryByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new HackerNewsStory
            {
                Title = $"Story {id}",
                Url = "http://test.com",
                By = "user",
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Score = id,
                Descendants = 0
            });

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new StoryService(mockClient.Object, memoryCache);

        var result = await service.GetBestStoriesAsync(2);

        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public async Task GetBestStoriesAsync_ShouldOrderByScoreDescending()
    {
        var mockClient = new Mock<IHackerNewsClient>();

        mockClient.Setup(c => c.GetBestStoryIdsAsync())
            .ReturnsAsync(new List<int> { 1, 2, 3 });

        mockClient.Setup(c => c.GetStoryByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new HackerNewsStory
            {
                Title = $"Story {id}",
                Url = "http://test.com",
                By = "user",
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Score = id,
                Descendants = 0
            });

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new StoryService(mockClient.Object, memoryCache);

        var result = await service.GetBestStoriesAsync(3);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].Score >= result[1].Score);
        Assert.True(result[1].Score >= result[2].Score);
    }
    
    [Fact]
    public async Task GetBestStoriesAsync_ShouldIgnoreNullStories()
    {
        var mockClient = new Mock<IHackerNewsClient>();

        mockClient.Setup(c => c.GetBestStoryIdsAsync())
            .ReturnsAsync(new List<int> { 1, 2 });

        mockClient.Setup(c => c.GetStoryByIdAsync(1))
            .ReturnsAsync(new HackerNewsStory
            {
                Title = "Valid Story",
                Url = "http://test.com",
                By = "user",
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Score = 100,
                Descendants = 10
            });

        mockClient.Setup(c => c.GetStoryByIdAsync(2))
            .ReturnsAsync((HackerNewsStory?)null);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new StoryService(mockClient.Object, memoryCache);

        var result = await service.GetBestStoriesAsync(2);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturnEmptyList_WhenNoIds()
    {
        var mockClient = new Mock<IHackerNewsClient>();

        mockClient.Setup(c => c.GetBestStoryIdsAsync())
            .ReturnsAsync(new List<int>());

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new StoryService(mockClient.Object, memoryCache);

        var result = await service.GetBestStoriesAsync(5);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task GetBestStoriesAsync_ShouldUseCache()
    {
        var mockClient = new Mock<IHackerNewsClient>();

        mockClient.Setup(c => c.GetBestStoryIdsAsync())
            .ReturnsAsync(new List<int> { 1 });

        mockClient.Setup(c => c.GetStoryByIdAsync(1))
            .ReturnsAsync(new HackerNewsStory
            {
                Title = "Cached Story",
                Url = "http://test.com",
                By = "user",
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Score = 50,
                Descendants = 5
            });

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new StoryService(mockClient.Object, memoryCache);

        await service.GetBestStoriesAsync(1);
        await service.GetBestStoriesAsync(1);

        mockClient.Verify(c => c.GetStoryByIdAsync(1), Times.Once);
    }
}