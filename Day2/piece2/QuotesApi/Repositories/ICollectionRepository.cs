using QuotesApi.Models;

namespace QuotesApi.Repositories;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Collection collection, CancellationToken ct = default);
    Task UpdateAsync(Collection collection, CancellationToken ct = default);
}
