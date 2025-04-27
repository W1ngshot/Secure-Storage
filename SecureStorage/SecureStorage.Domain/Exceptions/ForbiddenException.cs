using System.Net;
using SecureStorage.Domain.Exceptions.Codes;

namespace SecureStorage.Domain.Exceptions;

public class ForbiddenException() : DomainException(ErrorCodes.Forbidden, (int)HttpStatusCode.Forbidden);