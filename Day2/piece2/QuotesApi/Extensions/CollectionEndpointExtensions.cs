using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Extensions;

public static class CollectionEndpointExtensions
{
    public static void MapCollectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/collections");

        group.MapPost("/", async ([FromQuery] string name, [FromQuery] string ownerId, ICollectionRepository repo, CancellationToken ct) =>
        {
            var collection = new Collection(name, ownerId);
            await repo.AddAsync(collection, ct); // Pass token to repository!
            return Results.Created($"/api/collections/{collection.Id}", collection);
        });

        group.MapPost("/{id:int}/items/{quoteId:int}", async (int id, int quoteId, ICollectionRepository repo, CancellationToken ct) =>
        {
            var collection = await repo.GetByIdAsync(id, ct);
            if (collection is null) return Results.NotFound();

            try
            {
                collection.AddItem(quoteId);
                await repo.UpdateAsync(collection, ct);
                return Results.Ok(collection);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: "Domain Validation Failed");
            }
        });
    }
}
