using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Repositories;
using QuotesApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=quotes.db"));
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Auto-create the database for testing
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapCollectionEndpoints();
app.Run();
