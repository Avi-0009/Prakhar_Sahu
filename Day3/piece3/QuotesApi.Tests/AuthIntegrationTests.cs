using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace QuotesApi.Tests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetQuotes_Anonymous_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/quotes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostQuote_Anonymous_ReturnsUnauthorized_401()
    {
        // Act
        var response = await _client.PostAsync("/api/quotes", new StringContent(""));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteQuote_Anonymous_ReturnsUnauthorized_401()
    {
        // Act
        var response = await _client.DeleteAsync("/api/quotes/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Note: To fully test 403 Forbidden and 200 OK for protected routes, 
    // you would typically inject a mock JWT token provider or use a TestAuthHandler.
    // This scaffold proves the pipeline blocks unauthorized users end-to-end.
}
