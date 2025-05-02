namespace SecureStorage.Infrastructure.Options;

public class HashiCorpVaultOptions
{
    public const string SectionName = "Vault";
    
    public string ConnectionString { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}