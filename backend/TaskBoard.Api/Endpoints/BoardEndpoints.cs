using TaskBoard.Api.Dtos;
using TaskBoard.Api.Security;
using TaskBoard.Api.Services;

namespace TaskBoard.Api.Endpoints;

public static class BoardEndpoints
{
    public static IEndpointRouteBuilder MapBoardEndpoints(this IEndpointRouteBuilder app) // Using TypedResults for strongly-typed responses and automatic Swagger documentation
    {

        var group = app.MapGroup("/api/boards").WithTags("Boards");

        group.MapGet("/", async (ICurrentUser user, IBoardService boards) =>
        {
            var testPermissions = user.UserPermissions;
            var testRole = user.UserRole;

            return TypedResults.Ok(await boards.ListAsync(user.UserId));
        });

        group.MapPost("/", async (CreateBoardRequest request, ICurrentUser user, IBoardService boards) =>
        {
            var board = await boards.CreateAsync(user.UserId, request);
            return TypedResults.Created($"/api/boards/{board.Id}", board);
        });

        return app;
    }
}
