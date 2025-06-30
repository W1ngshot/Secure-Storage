using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.API.ExceptionHandling;
using SecureStorage.API.Middleware;
using SecureStorage.API.Processors;
using SecureStorage.API.ServiceExtensions;
using SecureStorage.Domain.Persistence;
using SecureStorage.Infrastructure.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddCustomLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRocksDbStorage(builder.Configuration);
builder.Services.AddKeyVault(builder.Configuration);
builder.Services.AddUtilityServices();
builder.Services.AddSecurityServices();
builder.Services.AddFastEndpointsConfiguration();
builder.Services.AddCoreHandlers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(_ => { });

app.UseFastEndpoints(config =>
{
    config.Errors.UseProblemDetails();
    config.Endpoints.Configurator = ep => { ep.PreProcessor<UserIdExtractorPreProcessor>(Order.Before); };
});

app.MapGet("/debug/rocksdb", ([FromServices] IKeyValueStorage keyValueStorage) =>
{
    var data = keyValueStorage
        .DumpAll()
        .Select(kv => new { kv.Key, kv.Value });

    return Results.Ok(data);
});

app.Run();