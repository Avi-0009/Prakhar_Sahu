using System;

namespace QuotesApi.Models;

public class Quote
{
    // Private setters: The outside world can read, but cannot blindly modify.
    public int Id { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // Required by Entity Framework Core for reflection
    private Quote() { }

    private Quote(string author, string text, DateTimeOffset createdAt)
    {
        Author = author;
        Text = text;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    // Static Factory Method enforcing invariants at the moment of creation
    public static Result<Quote> Create(string author, string text, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 1000)
            return Result<Quote>.Failure(new DomainError("Text must be between 1 and 1000 characters."));
            
        if (string.IsNullOrWhiteSpace(author) || author.Length > 200)
            return Result<Quote>.Failure(new DomainError("Author must be between 1 and 200 characters."));

        return Result<Quote>.Success(new Quote(author, text, createdAt));
    }

    // Text is immutable, but we allow changing the author
    public Result<bool> ChangeAuthor(string newAuthor)
    {
        if (IsDeleted) 
            return Result<bool>.Failure(new DomainError("Cannot modify a deleted quote."));
            
        if (string.IsNullOrWhiteSpace(newAuthor) || newAuthor.Length > 200)
            return Result<bool>.Failure(new DomainError("Author must be between 1 and 200 characters."));
        
        Author = newAuthor;
        return Result<bool>.Success(true);
    }

    // Soft delete encapsulation
    public void Delete()
    {
        IsDeleted = true;
    }
}
