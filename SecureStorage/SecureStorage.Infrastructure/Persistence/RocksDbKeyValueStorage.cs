using System.Text;
using Microsoft.Extensions.Options;
using RocksDbSharp;
using SecureStorage.Domain.Persistence;
using SecureStorage.Infrastructure.Options;

namespace SecureStorage.Infrastructure.Persistence;

public class RocksDbKeyValueStorage : IKeyValueStorage
{
    private readonly TransactionDb _db;

    public RocksDbKeyValueStorage(IOptions<RocksDbOptions> options)
    {
        Directory.CreateDirectory(options.Value.DatabasePath);
        var dbOptions = new DbOptions().SetCreateIfMissing();
        var txnOptions = new TransactionDbOptions();
        _db = TransactionDb.Open(dbOptions, txnOptions, options.Value.DatabasePath);
    }

    public Task<string?> GetAsync(string key) => Task.FromResult(_db.Get(key))!;

    public Task<Dictionary<string, string?>> GetBatchAsync(IEnumerable<string> keys)
    {
        var keyArray = keys.ToArray();
        var values = _db.MultiGet(keyArray)?.ToDictionary(k => k.Key, v => v.Value);

        return Task.FromResult(
            keyArray.ToDictionary(key => key, key => values?.GetValueOrDefault(key)));
    }

    public Task PutAsync(string key, string value)
    {
        _db.Put(key, value);
        return Task.CompletedTask;
    }

    public Task PutBatchAsync(Dictionary<string, string> batch)
    {
        var b = new WriteBatch();

        foreach (var (key, val) in batch)
            b.Put(key, val);

        _db.Write(b);
        return Task.CompletedTask;
    }


    public IEnumerable<(string Key, string Value)> DumpAll()
    {
        using var it = _db.NewIterator();
        it.SeekToFirst();

        while (it.Valid())
        {
            yield return (
                Encoding.UTF8.GetString(it.Key()),
                Encoding.UTF8.GetString(it.Value())
            );
            it.Next();
        }
    }

    public IStorageTransaction BeginTransaction() => new StorageTransaction(_db.BeginTransaction());

    public void Dispose() => _db.Dispose();
}