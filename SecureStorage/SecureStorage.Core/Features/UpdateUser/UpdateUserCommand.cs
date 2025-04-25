namespace SecureStorage.Core.Features.UpdateUser;

public class UpdateUserCommand
{
    public required string UserId { get; init; }
    public Dictionary<string, string> Level1Updates { get; init; } = new();
    public Dictionary<string, string> Level2Updates { get; init; } = new();
    public string? Password { get; init; }
}