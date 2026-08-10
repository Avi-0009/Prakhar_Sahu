using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Repositories;
using System;

namespace QuotesApi.Extensions;

public static class CollectionEndpointExtensions
{
    public static void MapCollectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/collections");

        group.MapPost("/", async (string name, string ownerId, ICollectionRepository repo, CancellationToken ct) =>
        {
            try {
                var collection = new Collection(name, ownerId);
                await repo.AddAsync(collection, ct);
                return Results.Created("/api/collections/" + collection.Id, collection);
            } 
            catch (ArgumentException ex) {
                return Results.Problem(statusCode: 400, title: "Domain Validation Failed", detail: ex.Message);
            }
        });

        group.MapPost("/{id:int}/items/{quoteId:int}", async (int id, int quoteId, ICollectionRepository repo, CancellationToken ct) =>
        {
            var collection = await repo.GetByIdAsync(id, ct);
            if (collection is null) return Results.NotFound();

            try {
                // ALL MUTATION GOES THROUGH THE AGGREGATE ROOT
                collection.AddItem(quoteId);
                await repo.UpdateAsync(collection, ct);
                return Results.Ok(collection);
            }
            catch (InvalidOperationException ex) {
                return Results.Problem(statusCode: 400, title: "Domain Validation Failed", detail: ex.Message);
            }
        });
    }
}
