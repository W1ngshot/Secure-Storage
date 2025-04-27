using System.Net;
using SecureStorage.Domain.Exceptions.Codes;

namespace SecureStorage.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException(int failedAttemptCount)
        : base(ErrorCodes.InvalidPassword, (int)HttpStatusCode.Forbidden)
    {
        PlaceholderData["failedAttempt"] = failedAttemptCount.ToString();
    }
}