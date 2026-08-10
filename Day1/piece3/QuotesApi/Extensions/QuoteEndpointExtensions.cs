using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Extensions;

public static class QuoteEndpointExtensions
{
    public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quotes");

        // GET /api/quotes?page=N&size=N
        group.MapGet("/", async (int? page, int? size, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            var p = page ?? 1;
            var s = size ?? 10;
            logger.LogInformation("Fetching quotes page {Page} with size {Size}", p, s);
            var (items, total) = await repo.GetPagedAsync(p, s, ct);
            return Results.Ok(new { Items = items, TotalCount = total, Page = p, Size = s });
        });

        // POST /api/quotes
        group.MapPost("/", async (CreateQuoteDto dto, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(dto.Author))
                errors.Add(nameof(dto.Author), new[] { "Author is required." });
            if (string.IsNullOrWhiteSpace(dto.Text))
                errors.Add(nameof(dto.Text), new[] { "Text is required." });

            if (errors.Count > 0)
            {
                logger.LogWarning("Validation failed for quote creation.");
                return Results.ValidationProblem(errors);
            }

            var quote = new Quote { Author = dto.Author, Text = dto.Text };
            var created = await repo.CreateAsync(quote, ct);
            logger.LogInformation("Created quote with ID {Id}", created.Id);
            return Results.Created($"/api/quotes/{created.Id}", created);
        });

        // GET /api/quotes/{id}
        group.MapGet("/{id:int}", async (int id, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            logger.LogInformation("Fetching quote with ID {Id}", id);
            var quote = await repo.GetByIdAsync(id, ct);
            return quote is not null ? Results.Ok(quote) : Results.NotFound();
        });

        // DELETE /api/quotes/{id}
        group.MapDelete("/{id:int}", async (int id, IQuoteRepository repo, ILogger<Program> logger, CancellationToken ct) =>
        {
            logger.LogInformation("Deleting quote with ID {Id}", id);
            var deleted = await repo.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}