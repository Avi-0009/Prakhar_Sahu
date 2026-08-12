using System;
using FluentAssertions;
using QuotesApi.Models;
using Xunit;

namespace QuotesApi.Tests.Unit;

public class QuoteTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WhenTextIsInvalid_ReturnsFailureResult(string invalidText)
    {
        // Arrange
        var author = "Valid Author";
        var createdAt = DateTimeOffset.UtcNow;
        var userId = 1;

        // Act
        var result = Quote.Create(author, invalidText, createdAt, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Text must be between 1 and 1000 characters");
    }

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessResultAndCreatesQuote()
    {
        // Arrange
        var author = "Marcus Aurelius";
        var text = "Amor Fati";
        var createdAt = DateTimeOffset.UtcNow;
        var userId = 1;

        // Act
        var result = Quote.Create(author, text, createdAt, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Author.Should().Be(author);
        result.Value.Text.Should().Be(text);
        result.Value.UserId.Should().Be(userId);
        result.Value.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ChangeAuthor_WhenValid_UpdatesAuthorAndReturnsSuccess()
    {
        // Arrange
        var initialQuote = Quote.Create("Old Author", "Some text", DateTimeOffset.UtcNow, 1).Value!;
        var newAuthor = "New Author";

        // Act
        var result = initialQuote.ChangeAuthor(newAuthor);

        // Assert
        result.IsSuccess.Should().BeTrue();
        initialQuote.Author.Should().Be(newAuthor);
    }
}
