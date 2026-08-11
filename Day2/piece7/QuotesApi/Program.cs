using QuotesApi.Extensions;
using QuotesApi.Endpoints;
using QuotesApi.Data;
using QuotesApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    
    // Seed a test user
    if (!db.Users.Any())
    {
        db.Users.Add(new User 
        { 
            Email = "admin@test.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") 
        });
        db.SaveChanges();
    }
}

// Middleware MUST be in this exact order
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapQuoteEndpoints();

app.Run();
