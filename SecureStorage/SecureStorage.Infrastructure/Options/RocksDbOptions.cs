namespace SecureStorage.Infrastructure.Options;

public class RocksDbOptions
{
    public const string SectionName = "RocksDb";
    
    public string DatabasePath { get; init; } = string.Empty;
}