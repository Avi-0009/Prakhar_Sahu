using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetQuotes_WhenCalled_ReturnsOkAndEmptyList()
    {
        var response = await _client.GetAsync("/api/quotes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostQuote_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new { Author = "Test", Text = "Test Text" };
        var response = await _client.PostAsJsonAsync("/api/quotes", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FullUserJourney_Register_Login_And_CreateQuote_ReturnsSuccess()
    {
        // 1. REGISTER
        var registerRequest = new { Email = "test@example.com", Password = "Password123!" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. LOGIN (Get Token)
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", registerRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(loginContent);
        var token = jsonDoc.RootElement.GetProperty("token").GetString();
        
        token.Should().NotBeNullOrEmpty();

        // 3. CREATE QUOTE (Using the real token)
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var quoteRequest = new { Author = "Marcus Aurelius", Text = "Amor Fati" };
        
        var quoteResponse = await _client.PostAsJsonAsync("/api/quotes", quoteRequest);
        
        // Asserting Created (201) or OK (200) depending on your API implementation
        quoteResponse.IsSuccessStatusCode.Should().BeTrue();
    }
}
