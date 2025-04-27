using System.Net;
using SecureStorage.Domain.Exceptions.Codes;

namespace SecureStorage.Domain.Exceptions;

public class LockedException : DomainException
{
    public LockedException(DateTime lockUntil)
        : base(ErrorCodes.Locked, (int)HttpStatusCode.Locked)
    {
        PlaceholderData["lockUntil"] = lockUntil.ToString("o");
    }
}