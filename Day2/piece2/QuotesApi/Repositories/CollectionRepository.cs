using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly AppDbContext _db;
    public CollectionRepository(AppDbContext db) => _db = db;

    public async Task<Collection?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Collections.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Collection collection, CancellationToken ct = default)
    {
        _db.Collections.Add(collection);
        
        // SIMULATE A LONG I/O OPERATION (5 seconds)
        // If the token is cancelled, this will immediately throw a TaskCanceledException
        await Task.Delay(5000, ct); 
        
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Collection collection, CancellationToken ct = default)
    {
        _db.Collections.Update(collection);
        await _db.SaveChangesAsync(ct);
    }
}
