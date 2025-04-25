using Microsoft.Extensions.DependencyInjection;
using SecureStorage.Domain.Vault;
using SecureStorage.Infrastructure.Vault;

namespace SecureStorage.Infrastructure.ServiceExtensions;

public static class VaultServiceExtensions
{
    public static IServiceCollection AddKeyVault(this IServiceCollection services)
    {
        services.AddSingleton<IKeyVault, MockKeyVault>();
        return services;
    }
}