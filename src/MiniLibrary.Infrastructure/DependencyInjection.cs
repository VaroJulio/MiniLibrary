using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Configuration;
using MiniLibrary.Infrastructure.Data;
using MiniLibrary.Infrastructure.Repositories;
using MiniLibrary.Infrastructure.Services;

namespace MiniLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Domain event dispatcher (must be registered before DbContext so it can be injected)
        services.AddScoped<DomainEventDispatcher>();

        services.AddDbContext<AppDbContext>((sp, options) =>
            options
                .UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatcher>()));

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // JWT Token Service
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // OpenAI Embedding Service
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
        services.AddScoped<IRecommendationService, OpenAiRecommendationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IRankingsService, RankingsService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Repositories
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IBadgeRepository, BadgeRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookEmbeddingRepository, BookEmbeddingRepository>();
        services.AddScoped<IReviewVoteRepository, ReviewVoteRepository>();

        return services;
    }
}
