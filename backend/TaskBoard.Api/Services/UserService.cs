using TaskBoard.Api.Domain;
using TaskBoard.Api.Domain.Exceptions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Api.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}

public class UserService(IUserRepository users) : IUserService
{
    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        if (await users.GetByEmailAsync(request.Email ?? string.Empty) is not null)
            throw new BusinessRuleException("Email already registered.");

        // No credentials for now: PasswordHash stays empty until the real auth flow exists.
        var user = User.Create(request.Name, request.Email!, passwordHash: string.Empty);
        await users.AddAsync(user);
        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
