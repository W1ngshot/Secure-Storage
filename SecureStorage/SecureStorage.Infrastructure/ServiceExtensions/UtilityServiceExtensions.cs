using Microsoft.Extensions.DependencyInjection;
using SecureStorage.Domain.Utility;
using SecureStorage.Infrastructure.Utility;

namespace SecureStorage.Infrastructure.ServiceExtensions;

public static class UtilityServiceExtensions
{
    public static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}