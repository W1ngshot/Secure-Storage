namespace SecureStorage.Domain.Utility;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}