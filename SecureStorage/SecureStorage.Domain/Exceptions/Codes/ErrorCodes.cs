namespace SecureStorage.Domain.Exceptions.Codes;

public static class ErrorCodes
{
    public const string NotFound = "error.not_found";
    public const string Forbidden = "error.forbidden";
    public const string Locked = "error.locked";
    public const string InvalidPassword = "error.invalid_password";
    public const string CorruptedEntity = "error.corrupted";
    
    public const string InternalError = "error.internal";
}