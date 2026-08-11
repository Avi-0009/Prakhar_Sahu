using QuotesApi.Extensions;
using QuotesApi.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();

// Recreate the database so the new CreatedAt column exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapQuoteEndpoints();
app.Run();
