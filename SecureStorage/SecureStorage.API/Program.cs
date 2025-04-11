using System.Text;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Infrastructure.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRocksDbStorage(builder.Configuration);
builder.Services.AddSecurityServices();

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

app.MapPost("/encrypt", (
    EncryptRequest req,
    IEncryptionService encryption) =>
{
    var key = Convert.FromBase64String(req.Base64Key);
    var plainBytes = Encoding.UTF8.GetBytes(req.PlainText);
    var encrypted = encryption.Encrypt(plainBytes, key);
    return Results.Ok(Convert.ToBase64String(encrypted));
});

app.MapPost("/decrypt", (
    DecryptRequest req,
    IEncryptionService encryption) =>
{
    var key = Convert.FromBase64String(req.Base64Key);
    var cipherBytes = Convert.FromBase64String(req.Base64CipherText);
    var decrypted = encryption.Decrypt(cipherBytes, key);
    return Results.Ok(Encoding.UTF8.GetString(decrypted));
});

app.MapPost("/kdf", (
    KdfRequest req,
    IKdfService kdf) =>
{
    var salt = Convert.FromBase64String(req.Base64Salt);
    var derivedKey = kdf.DeriveKey(req.Password, salt);
    return Results.Ok(Convert.ToBase64String(derivedKey));
});

app.Run();

public record EncryptRequest(string PlainText, string Base64Key);
public record DecryptRequest(string Base64CipherText, string Base64Key);

public record KdfRequest(string Password, string Base64Salt);