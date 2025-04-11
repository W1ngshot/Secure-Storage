namespace SecureStorage.Domain.Persistence;

public interface IKeyValueStorage : IDisposable
{
    Task<string?> GetAsync(string key);
    Task<Dictionary<string, string?>> GetBatchAsync(IEnumerable<string> keys);
    Task PutAsync(string key, string value);
    Task PutBatchAsync(Dictionary<string, string> batch);
    IStorageTransaction BeginTransaction();
}