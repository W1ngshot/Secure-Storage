using Microsoft.Extensions.DependencyInjection;
using SecureStorage.Core.Interfaces.Security;
using SecureStorage.Infrastructure.Security;

namespace SecureStorage.Infrastructure.ServiceExtensions;

public static class SecurityServicesRegistrationExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddSingleton<IKdfService, Pbkdf2KdfService>();
        services.AddSingleton<IKeyGenerator, KeyGenerator>();
        return services;
    }
}