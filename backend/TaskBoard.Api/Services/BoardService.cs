using TaskBoard.Api.Domain;
using TaskBoard.Api.Domain.Exceptions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Api.Services;

public interface IBoardService
{
    Task<IReadOnlyList<BoardResponse>> ListAsync(Guid userId);
    Task<BoardResponse> CreateAsync(Guid userId, CreateBoardRequest request);
}

public class BoardService(IBoardRepository boards, IUserRepository users) : IBoardService
{
    public async Task<IReadOnlyList<BoardResponse>> ListAsync(Guid userId) =>
        (await boards.ListForUserAsync(userId)).Select(b => b.ToResponse()).ToList();

    public async Task<BoardResponse> CreateAsync(Guid userId, CreateBoardRequest request)
    {
        if (!await users.ExistsAsync(userId))
            throw new UnauthorizedException("Caller does not exist.");

        var board = Board.Create(userId, request.Name);
        await boards.AddAsync(board);
        return board.ToResponse();
    }
}
