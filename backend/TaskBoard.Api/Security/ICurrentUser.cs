using TaskBoard.Api.Domain.Exceptions;

namespace TaskBoard.Api.Security;

/// <summary>Who is calling. Services receive the user id explicitly, so they never touch HttpContext.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
}

/// <summary>
/// PLACEHOLDER, not authentication: trusts the X-User-Id header so the owner/assignee rules can run.
/// Replace with an implementation backed by validated token claims once simple BCrypt/JWT auth is in place.
/// </summary>
public class HeaderCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public const string HeaderName = "X-User-Id";

    public Guid UserId =>
        Guid.TryParse(accessor.HttpContext?.Request.Headers[HeaderName].ToString(), out var id)
            ? id
            : throw new UnauthorizedException($"Missing or invalid {HeaderName} header.");
}
