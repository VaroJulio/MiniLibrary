using Microsoft.Extensions.DependencyInjection;

namespace MiniLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // EF Core, repositories, and external services will be registered here
        return services;
    }
}
