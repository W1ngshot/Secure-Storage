using SecureStorage.API.Models.Base;

namespace SecureStorage.API.Models;

public class UpdateUserRequest : BaseRequest
{
    public Dictionary<string, string> Level1Updates { get; init; } = new();
    public Dictionary<string, string> Level2Updates { get; init; } = new();
    public string? Password { get; init; }
}