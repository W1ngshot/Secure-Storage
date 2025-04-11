using SecureStorage.Domain.Persistence;
using SecureStorage.Infrastructure.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRocksDbStorage(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/put", async (IKeyValueStorage storage, string key, string value) =>
{
    await storage.PutAsync(key, value);
    return Results.Ok($"Stored {key} = {value}");
});

app.MapPost("/put-batch", async (IKeyValueStorage storage, Dictionary<string, string> data) =>
{
    await storage.PutBatchAsync(data);
    return Results.Ok("Batch written");
});

app.MapGet("/get", async (IKeyValueStorage storage, string key) =>
{
    var value = await storage.GetAsync(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
});

app.MapPost("/get-batch", async (IKeyValueStorage storage, List<string> keys) =>
{
    var values = await storage.GetBatchAsync(keys);
    return Results.Ok(values);
});

app.MapPost("/transaction", (IKeyValueStorage storage) =>
{
    using var txn = storage.BeginTransaction();
    txn.Put("txn-key1", "txn-value1");
    txn.Put("txn-key2", "txn-value2");
    txn.Delete("some-key-to-delete");
    txn.Commit();
    return Results.Ok("Transaction committed");
});

app.Run();