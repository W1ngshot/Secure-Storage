using RocksDbSharp;
using SecureStorage.Core.Interfaces.Persistence;

namespace SecureStorage.Infrastructure.Persistence;

public class StorageTransaction(Transaction txn) : IStorageTransaction
{
    public string? Get(string key) => txn.Get(key);

    public Dictionary<string, string?> GetBatch(IEnumerable<string> keys)
    {
        var values = new Dictionary<string, string?>();
        foreach (var key in keys)
        {
            values[key] = txn.Get(key);
        }

        return values;
    }

    public void Put(string key, string value) => txn.Put(key, value);

    public void PutBatch(Dictionary<string, string> batch)
    {
        foreach (var (key, value) in batch)
            txn.Put(key, value);
    }

    public void Delete(string key) => txn.Remove(key);
    public void Rollback() => txn.Rollback();

    public void Commit() => txn.Commit();

    public void Dispose() => txn.Dispose();
}