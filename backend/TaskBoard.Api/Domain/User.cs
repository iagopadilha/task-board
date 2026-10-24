using BCrypt.Net;
using TaskBoard.Api.Domain.Exceptions;

namespace TaskBoard.Api.Domain;

public class User
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 254;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    private User() { }

    public static User Create(string name, string email, string passwordHash)
    {
        name = name?.Trim() ?? string.Empty;
        email = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (name.Length is 0 or > NameMaxLength)
            throw new DomainValidationException($"Name is required and must have at most {NameMaxLength} characters.");
        if (email.Length > EmailMaxLength || !email.Contains('@'))
            throw new DomainValidationException("A valid email is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = passwordHash
        };
    }
}
