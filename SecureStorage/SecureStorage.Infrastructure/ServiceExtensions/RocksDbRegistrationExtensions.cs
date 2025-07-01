using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureStorage.Core.Interfaces.Persistence;
using SecureStorage.Infrastructure.Options;
using SecureStorage.Infrastructure.Persistence;

namespace SecureStorage.Infrastructure.ServiceExtensions;

public static class RocksDbRegistrationExtensions
{
    public static IServiceCollection AddRocksDbStorage(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RocksDbOptions>(config.GetRequiredSection(RocksDbOptions.SectionName));

        services.AddSingleton<IKeyValueStorage, RocksDbKeyValueStorage>();

        return services;
    }
}