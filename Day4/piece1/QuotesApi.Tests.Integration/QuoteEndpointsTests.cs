using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;
using QuotesApi.Models;
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
    public async Task FullUserJourney_SeedUser_Login_Refresh_And_CreateQuote_ReturnsSuccess()
    {
        // 1. SEED TEST USER
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = new User 
            { 
                Email = "test@example.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") 
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        // 2. LOGIN
        var loginRequest = new { Email = "test@example.com", Password = "Password123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        
        var loginDict = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var refreshToken = loginDict!.FirstOrDefault(k => k.Key.Equals("refreshToken", StringComparison.OrdinalIgnoreCase)).Value.GetString();

        // 3. REFRESH TOKEN
        var refreshRequest = new { RefreshToken = refreshToken };
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        refreshResponse.EnsureSuccessStatusCode();
        
        var refreshDict = await refreshResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var newAccessToken = refreshDict!.FirstOrDefault(k => k.Key.Equals("accessToken", StringComparison.OrdinalIgnoreCase)).Value.GetString();

        // 4. CREATE QUOTE
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
        var quoteRequest = new { Author = "Marcus Aurelius", Text = "Amor Fati" };
        var quoteResponse = await _client.PostAsJsonAsync("/api/quotes", quoteRequest);
        
        // This will print the EXACT error message from the API if it fails!
        var errorBody = await quoteResponse.Content.ReadAsStringAsync();
        quoteResponse.IsSuccessStatusCode.Should().BeTrue("API rejected quote creation. Status: {0}, Error: {1}", quoteResponse.StatusCode, errorBody);
    }
}
