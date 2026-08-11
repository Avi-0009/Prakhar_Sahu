using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly AppDbContext _db;

    public QuoteRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Quote>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Quotes.AsNoTracking().ToListAsync(ct);

    public async Task<Quote?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Quotes.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(Quote quote, CancellationToken ct = default)
    {
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Quote quote, CancellationToken ct = default)
    {
        _db.Quotes.Update(quote);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Quote quote, CancellationToken ct = default)
    {
        _db.Quotes.Remove(quote);
        await _db.SaveChangesAsync(ct);
    }
}
