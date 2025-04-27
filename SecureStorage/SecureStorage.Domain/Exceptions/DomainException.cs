namespace SecureStorage.Domain.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public Dictionary<string, string> PlaceholderData { get; } = new();

    protected DomainException(string errorCode, int statusCode = 500) : base(errorCode)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}