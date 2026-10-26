using System.Collections.ObjectModel;
using System.Security.Claims;
using TaskBoard.Api.Domain.Exceptions;

namespace TaskBoard.Api.Security;

/// <summary>Who is calling. Services receive the user id explicitly, so they never touch HttpContext.</summary>
public interface ICurrentUser
{
    Guid UserId { get; } // syntetic sugar. Acess like _interface.UserId and gets data like we're doing below
    string UserName { get; }
    string UserEmail { get; }
    string UserRole { get; }
    ReadOnlyCollection<string> UserPermissions { get; }
}

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier); // same as sub/id
            return Guid.TryParse(sub, out var id)
                ? id
                : throw new UnauthorizedException("Invalid or missing user identifier claim in token.");
        }
    }

    public string UserName
    {
        get
        {
            var userName = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            return !string.IsNullOrEmpty(userName) ? userName : throw new UnauthorizedException("Invalid or missing user identifier claim in token.");
        }
    }

    public string UserEmail
    {
        get
        {
            var userEmail = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
            return !string.IsNullOrEmpty(userEmail) ? userEmail : throw new UnauthorizedException("Invalid or missing user identifier claim in token.");
        }
    } // Claims can be customized - 

    public string UserRole
    {
        get
        {
            var userRole = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return !string.IsNullOrEmpty(userRole) ? userRole : throw new UnauthorizedException("Invalid or missing user identifier claim in token.");
        }
    }

    public ReadOnlyCollection<string> UserPermissions
    {
        get
        {
            var userPemissions = accessor.HttpContext?.User.FindAll("permission").Select(x => x.Value).ToList().AsReadOnly();
            return userPemissions != null && userPemissions.Any() ? userPemissions : throw new UnauthorizedException("Invalid or missing user identifier claim in token.");
        }
    }
}
