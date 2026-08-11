using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Repositories;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Collection collection, CancellationToken ct);
    Task UpdateAsync(Collection collection, CancellationToken ct);
}

public class CollectionRepository : ICollectionRepository
{
    private readonly AppDbContext _db;
    public CollectionRepository(AppDbContext db) => _db = db;

    public Task<Collection?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Collections.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Collection collection, CancellationToken ct)
    {
        _db.Collections.Add(collection);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Collection collection, CancellationToken ct)
    {
        _db.Collections.Update(collection);
        await _db.SaveChangesAsync(ct);
    }
}
