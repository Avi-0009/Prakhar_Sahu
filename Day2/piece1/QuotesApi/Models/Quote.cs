using System;
namespace QuotesApi.Models;

public class Quote
{
    public int Id {get; set;}
    public string Text {get; set;} = string.Empty;
    public string Author { get; set;} = string.Empty;
    public DateTimeOffset CreatedAt {get; set;}
}