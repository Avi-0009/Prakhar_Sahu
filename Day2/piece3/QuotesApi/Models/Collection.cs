using System;
using System.Collections.Generic;
using System.Linq;

namespace QuotesApi.Models;

public class Collection
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    
    private readonly List<CollectionItem> _items = new();
    public IReadOnlyCollection<CollectionItem> Items => _items.AsReadOnly();

    // Required by Entity Framework Core
    private Collection() { }

    public Collection(string name, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name cannot be empty.", nameof(name));
            
        if (name.Length > 80) 
            throw new ArgumentException("Name cannot exceed 80 characters.", nameof(name));
        
        Name = name;
        OwnerId = ownerId;
    }

    public void AddItem(int quoteId)
    {
        if (_items.Count >= 50) 
            throw new InvalidOperationException("Collection cannot exceed 50 items.");
            
        if (_items.Any(i => i.QuoteId == quoteId)) 
            throw new InvalidOperationException("Quote is already in the collection.");
        
        // FIX: Passing both required arguments to the CollectionItem constructor
        _items.Add(new CollectionItem(quoteId, DateTime.UtcNow));
    }

    public void RemoveItem(int quoteId)
    {
        var item = _items.FirstOrDefault(i => i.QuoteId == quoteId);
        if (item == null) 
            throw new InvalidOperationException("Quote not found in collection.");
        
        _items.Remove(item);
    }
}
