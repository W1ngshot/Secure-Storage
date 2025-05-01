namespace SecureStorage.Domain.Entities;

public class Level2
{
    public required string Secret { get; init; }
    public Dictionary<string, string> Level2Fields { get; init; } = new();
}