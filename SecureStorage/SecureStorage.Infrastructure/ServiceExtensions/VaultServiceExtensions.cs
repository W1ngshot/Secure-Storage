using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SecureStorage.Domain.Vault;
using SecureStorage.Infrastructure.Options;
using SecureStorage.Infrastructure.Vault;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace SecureStorage.Infrastructure.ServiceExtensions;

public static class VaultServiceExtensions
{
    public static IServiceCollection AddKeyVault(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HashiCorpVaultOptions>(configuration.GetRequiredSection(HashiCorpVaultOptions.SectionName));

        services.AddSingleton<IVaultClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<HashiCorpVaultOptions>>().Value;
            var authMethod = new TokenAuthMethodInfo(options.Token);
            var vaultClientSettings = new VaultClientSettings(options.ConnectionString, authMethod);
            return new VaultClient(vaultClientSettings);
        });

        services.AddSingleton<IKeyVault, HashiCorpVault>();
        return services;
    }
}