using System;
using FluentAssertions;
using QuotesApi.Models;
using Xunit;

namespace QuotesApi.Tests.Domain;

public class CollectionTests
{
    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Action act = () => new Collection("  ", "user123");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNameOver80Chars_ThrowsArgumentException()
    {
        var longName = new string('A', 81);
        Action act = () => new Collection(longName, "user123");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_When51stItemAdded_ThrowsInvalidOperationException()
    {
        var collection = new Collection("My Favorites", "user123");
        for (int i = 1; i <= 50; i++) collection.AddItem(i);

        Action act = () => collection.AddItem(51);
        act.Should().Throw<InvalidOperationException>().WithMessage("*50 items*");
    }

    [Fact]
    public void AddItem_WithDuplicateQuoteId_ThrowsInvalidOperationException()
    {
        var collection = new Collection("My Favorites", "user123");
        collection.AddItem(1);

        Action act = () => collection.AddItem(1);
        act.Should().Throw<InvalidOperationException>().WithMessage("*already in the collection*");
    }

    [Fact]
    public void RemoveItem_WhenItemDoesNotExist_ThrowsInvalidOperationException()
    {
        var collection = new Collection("My Favorites", "user123");

        Action act = () => collection.RemoveItem(99);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public void AddThenRemoveItem_LeavesZeroItems()
    {
        var collection = new Collection("My Favorites", "user123");
        collection.AddItem(1);
        collection.RemoveItem(1);

        collection.Items.Should().BeEmpty();
    }
}
