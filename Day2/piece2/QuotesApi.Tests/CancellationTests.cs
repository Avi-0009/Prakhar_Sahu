using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System;

namespace QuotesApi.Tests;

public class CancellationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CancellationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostCollection_WhenCancelledMidRequest_ThrowsOperationCanceledException()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var cts = new CancellationTokenSource();

        // Act
        // We trigger the POST request but DO NOT await it immediately
        var requestTask = client.PostAsync("/api/collections?name=SlowTest&ownerId=123", null, cts.Token);

        // Immediately cancel the token to simulate a user closing their browser/refreshing
        cts.Cancel();

        // Assert
        // The HttpClient will throw because the CancellationToken was tripped before the 5 second delay finished
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await requestTask);
    }
}
