using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Core.Features.CreateUser;
using SecureStorage.Core.Features.GetFields;
using SecureStorage.Core.Features.UpdateUser;
using SecureStorage.Domain.Persistence;
using SecureStorage.Infrastructure.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRocksDbStorage(builder.Configuration);
builder.Services.AddKeyVault();
builder.Services.AddUtilityServices();
builder.Services.AddSecurityServices();
builder.Services.AddFastEndpoints();

builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<GetFieldsQueryHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseFastEndpoints();

app.MapGet("/debug/rocksdb", ([FromServices] IKeyValueStorage keyValueStorage) =>
{
    var data = keyValueStorage
        .DumpAll()
        .Select(kv => new { kv.Key, kv.Value });

    return Results.Ok(data);
});

app.Run();