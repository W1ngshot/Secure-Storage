namespace SecureStorage.Core.Features.GetFields;

public class GetFieldsQuery
{
    public required string UserId { get; init; }
    public string? Password { get; init; }
    public required string[] Fields { get; init; }
}