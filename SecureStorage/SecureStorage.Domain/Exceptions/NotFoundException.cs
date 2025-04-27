using System.Net;
using SecureStorage.Domain.Exceptions.Codes;

namespace SecureStorage.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string resource)
        : base(ErrorCodes.NotFound, (int)HttpStatusCode.NotFound)
    {
        PlaceholderData["resource"] = resource;
    }
}