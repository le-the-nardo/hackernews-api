using HackerNewsApi.Clients;
using HackerNewsApi.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<StoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx, 408, network failures
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromMilliseconds(500 * Math.Pow(2, retryAttempt))
        );
}

app.UseHttpsRedirection();

app.MapGet("/api/stories/best", async (int n, StoryService service) =>
{
    if (n <= 0)
        return Results.BadRequest("n must be greater than 0");
    
    var result = await service.GetBestStoriesAsync(n);

    return Results.Ok(result);
});

app.Run();
