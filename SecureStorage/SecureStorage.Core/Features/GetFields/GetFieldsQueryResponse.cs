namespace SecureStorage.Core.Features.GetFields;

public class GetFieldsQueryResponse
{
    public GetFieldsQueryResponse(IEnumerable<string> level1Query, IEnumerable<string> level2Query)
    {
        foreach (var level1Key in level1Query)
        {
            Level1Fields[level1Key] = null;
        }

        foreach (var level2Key in level2Query)
        {
            Level2Fields[level2Key] = null;
        }
    }

    public Dictionary<string, string?> Level1Fields { get; init; } = new();

    public Dictionary<string, string?> Level2Fields { get; init; } = new();
}