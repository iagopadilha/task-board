using TaskBoard.Api.Domain;
using TaskBoard.Api.Domain.Exceptions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Api.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<ReturnLogin> LoginAsync(CreateLoginDto loginDto);
}

public class UserService(IUserRepository _users, ITokenGeneratorService _tokenGeneratorService) : IUserService
{
    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        if (await _users.GetByEmailAsync(request.Email ?? string.Empty) is not null)
            throw new BusinessRuleException("Email already registered.");

        // hashing with bcrypt -> constant time + salt
        var passwordHashed = BCrypt.Net.BCrypt.HashPassword(request.password);


        var user = User.Create(request.Name, request.Email!, passwordHash: passwordHashed);
        await _users.AddAsync(user);
        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<ReturnLogin> LoginAsync(CreateLoginDto loginDto)
    {
        var userToLogIn = await _users.GetByEmailAsync(loginDto.email ?? string.Empty);
        if (userToLogIn is null) throw new BusinessRuleException("Email or password might be incorrect.");

        var passwordAssert = BCrypt.Net.BCrypt.Verify(loginDto.password, userToLogIn.PasswordHash);

        if (!passwordAssert) throw new BusinessRuleException("Email or password might be incorrect.");
        string jwtBearerToken = _tokenGeneratorService.CreateJwtToken(userToLogIn.Id, userToLogIn.Name, userToLogIn.Email);

        return !string.IsNullOrEmpty(jwtBearerToken) ? new ReturnLogin(jwtBearerToken) : throw new InvalidOperationException("Error creating JWT for current user!");
    }
}
