namespace SecureStorage.Core.Interfaces.Utility;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}