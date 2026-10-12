namespace TaskBoard.Api.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

/// <summary>Invalid input (HTTP 400).</summary>
public class DomainValidationException(string message) : DomainException(message);

/// <summary>Caller is not identified (HTTP 401).</summary>
public class UnauthorizedException(string message) : DomainException(message);

/// <summary>Caller is identified but not allowed (HTTP 403).</summary>
public class ForbiddenException(string message) : DomainException(message);

/// <summary>Resource does not exist (HTTP 404).</summary>
public class NotFoundException(string message) : DomainException(message);

/// <summary>Business rule violated: invalid transition, WIP limit, immutable task... (HTTP 409).</summary>
public class BusinessRuleException(string message) : DomainException(message);
