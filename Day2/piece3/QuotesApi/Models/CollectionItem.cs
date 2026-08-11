namespace QuotesApi.Models;

// VALUE OBJECT: Immutable, defined by its values, no distinct identity.
public record CollectionItem(int QuoteId, DateTime AddedAt);
