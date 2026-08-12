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
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetQuotes_WhenCalled_ReturnsOkAndEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/quotes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    [Fact]
    public async Task PostQuote_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        // FIX: Using an anonymous object to avoid DTO constructor mismatches!
        var request = new { Author = "Test", Text = "Test Text" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/quotes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
