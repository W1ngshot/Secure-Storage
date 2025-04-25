using SecureStorage.Domain.Utility;

namespace SecureStorage.Infrastructure.Utility;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}