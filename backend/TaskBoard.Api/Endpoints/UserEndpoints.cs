using TaskBoard.Api.Dtos;
using TaskBoard.Api.Services;

namespace TaskBoard.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app) // rodamos com typed results pq retorna tipado e tipa no swagger tambem
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        // Study project: open, password-less user creation until the real auth flow replaces it.
        group.MapPost("/", async (CreateUserRequest request, IUserService users) =>
        {
            var user = await users.CreateAsync(request);
            return TypedResults.Created($"/api/users/{user.Id}", user);
        });

        return app;
    }
}
    