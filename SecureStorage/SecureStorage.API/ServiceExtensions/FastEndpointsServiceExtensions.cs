using FastEndpoints;
using FastEndpoints.Swagger;

namespace SecureStorage.API.ServiceExtensions;

public static class FastEndpointsServiceExtensions
{
    public static IServiceCollection AddFastEndpointsConfiguration(this IServiceCollection services)
    {
        services
            .AddFastEndpoints()
            .SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "SecureStorage API";
                    s.Version = "v1";
                    s.Description = "API для безопасного хранения данных и управления учетной записью";
                };
            });

        return services;
    }
}