using SecureStorage.Domain.Exceptions.Codes;

namespace SecureStorage.Domain.Exceptions;

public class CorruptedEntityException() : DomainException(ErrorCodes.CorruptedEntity);