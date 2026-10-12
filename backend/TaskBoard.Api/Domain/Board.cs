using TaskBoard.Api.Domain.Exceptions;

namespace TaskBoard.Api.Domain;

public class Board
{
    public const int NameMaxLength = 100;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }

    private Board() { }

    public static Board Create(Guid ownerId, string name)
    {
        name = name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > NameMaxLength)
            throw new DomainValidationException($"Name is required and must have at most {NameMaxLength} characters.");

        return new Board { Id = Guid.NewGuid(), Name = name, OwnerId = ownerId };
    }
}
