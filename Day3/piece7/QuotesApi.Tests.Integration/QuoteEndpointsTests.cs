using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;
using QuotesApi.Dtos;
using Xunit;

namespace QuotesApi.Tests.Integration;

public class QuoteEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public QuoteEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Wipe and recreate the schema for EACH test to guarantee a fresh state
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetQuotes_WhenCalled_ReturnsOkAndEmptyList()
    {
        var response = await _client.GetAsync("/api/quotes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    [Fact]
    public async Task PostQuote_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new { Author = "Test", Text = "Test Text" };
        var response = await _client.PostAsJsonAsync("/api/quotes", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
