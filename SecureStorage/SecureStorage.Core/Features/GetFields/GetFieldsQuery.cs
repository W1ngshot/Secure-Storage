namespace SecureStorage.Core.Features.GetFields;

public class GetFieldsQuery
{
    public required string UserId { get; init; }
    public string? Password { get; init; }
    public required string[] Level1Fields { get; init; }
    public required string[] Level2Fields { get; init; }
}