using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Repositories;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("`DefaultConnection`") ?? "`Data Source=quotes.db`"));

        services.AddScoped<IQuoteRepository, QuoteRepository>();

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}