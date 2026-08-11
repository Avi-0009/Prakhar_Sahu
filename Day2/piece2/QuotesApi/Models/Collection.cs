using System;
using System.Collections.Generic;
using System.Linq;

namespace QuotesApi.Models;

// AGGREGATE ROOT: Controls all access to the items, enforces invariants.
public class Collection
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string OwnerId { get; private set; }
    
    private readonly List<CollectionItem> _items = new();
    public IReadOnlyList<CollectionItem> Items => _items.AsReadOnly();

    private Collection() { } // Required for EF Core

    public Collection(string name, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 80)
            throw new ArgumentException("Name must be between 3 and 80 characters.");
            
        Name = name;
        OwnerId = ownerId;
    }

    public void AddItem(int quoteId)
    {
        if (_items.Count >= 50)
            throw new InvalidOperationException("Collection cannot exceed 50 items.");
            
        if (_items.Any(i => i.QuoteId == quoteId))
            throw new InvalidOperationException("Quote " + quoteId + " is already in the collection.");

        _items.Add(new CollectionItem(quoteId, DateTime.UtcNow));
    }

    public void RemoveItem(int quoteId)
    {
        var item = _items.FirstOrDefault(i => i.QuoteId == quoteId);
        if (item != null)
            _items.Remove(item);
    }
}
