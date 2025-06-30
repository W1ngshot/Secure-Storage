using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace SecureStorage.API.ServiceExtensions;

public static class LoggingExtensions
{
    public static void AddCustomLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, _, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application",
                    context.Configuration["ApplicationName"] ?? throw new InvalidOperationException("no app name"))
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new Uri(context.Configuration["ElasticConfiguration:Uri"] ??
                            throw new InvalidOperationException("No es uri")))
                {
                    IndexFormat =
                        $"{context.Configuration["ApplicationName"]}-logs-{context.HostingEnvironment.EnvironmentName.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-mm}"
                });
        });
    }
}