using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;
using Testcontainers.MsSql;
using Xunit;

namespace QuotesApi.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddScoped<DbContextOptions<AppDbContext>>(sp =>
            {
                // FIX: Intercept the connection string and point it to a dedicated test DB 
                // so EF Core doesn't try to drop the system 'master' database.
                var csBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
                {
                    InitialCatalog = "QuotesTestDb"
                };

                return new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(csBuilder.ConnectionString)
                    .Options;
            });
        });
    }
}
