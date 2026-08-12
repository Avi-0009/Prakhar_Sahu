using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
        var response = await _client.GetAsync("/api/quotes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostQuote_Anonymous_ReturnsUnauthorized_401()
    {
        // Send actual JSON so it passes the Media Type check and hits the Auth check
        var json = "{\"author\":\"test\",\"text\":\"test\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/quotes", content);

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
}
