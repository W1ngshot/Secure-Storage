namespace SecureStorage.Domain.Entities;

public class Level2
{
    public required string Secret { get; set; }
    public Dictionary<string, string> Level2Fields { get; set; } = new();
}