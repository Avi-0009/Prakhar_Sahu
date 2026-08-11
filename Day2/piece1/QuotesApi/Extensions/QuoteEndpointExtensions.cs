using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Repositories;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class QuoteEndpointExtensions
{
    public static void MapQuoteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/quotes");

        // GET ALL
        group.MapGet("/", async (IQuoteRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetAllAsync(ct)));

        // GET BY ID
        group.MapGet("/{id:int}", async (int id, IQuoteRepository repo, CancellationToken ct) =>
            await repo.GetByIdAsync(id, ct) is Quote quote ? Results.Ok(quote) : Results.NotFound());

        // POST (INJECTING ICLOCK HERE)
        group.MapPost("/", async (Quote quote, IQuoteRepository repo, IClock clock, CancellationToken ct) =>
        {
            // Use the injected clock instead of DateTimeOffset.UtcNow!
            quote.CreatedAt = clock.UtcNow; 
            await repo.AddAsync(quote, ct);
            return Results.Created($"/api/quotes/{quote.Id}", quote);
        });

        // PUT (UPDATE)
        group.MapPut("/{id:int}", async (int id, Quote updatedQuote, IQuoteRepository repo, CancellationToken ct) =>
        {
            var quote = await repo.GetByIdAsync(id, ct);
            if (quote is null) return Results.NotFound();

            quote.Text = updatedQuote.Text;
            quote.Author = updatedQuote.Author;
            await repo.UpdateAsync(quote, ct);
            return Results.NoContent();
        });

        // DELETE
        group.MapDelete("/{id:int}", async (int id, IQuoteRepository repo, CancellationToken ct) =>
        {
            var quote = await repo.GetByIdAsync(id, ct);
            if (quote is null) return Results.NotFound();

            await repo.DeleteAsync(quote, ct);
            return Results.NoContent();
        });
    }
}
