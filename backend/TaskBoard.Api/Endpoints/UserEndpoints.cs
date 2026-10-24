using TaskBoard.Api.Dtos;
using TaskBoard.Api.Services;

namespace TaskBoard.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app) // Using TypedResults for strongly-typed responses and automatic Swagger documentation
    {
        var group = app.MapGroup("/api/users").WithTags("Users");


        group.MapPost("/", async (CreateUserRequest request, IUserService users) =>
        {
            var user = await users.CreateAsync(request);
            return TypedResults.Created($"/api/users/{user.Id}", user);
        });


        // [Authorize(AuthenticationSchemes = "FirebaseJwt")] Example of requiring a specific authentication scheme
        group.MapPost("/login", async (CreateLoginDto request, IUserService users, HttpContext httpContext) => 
        {
            var acessToken = await users.LoginAsync(request);

            httpContext.Response.Cookies.Append("acess_token", acessToken.jwtAcessToken, new CookieOptions // browser will set this automatically
            {
                HttpOnly = true,
                Secure = true, // only works over localhost or HTTPS
                SameSite = SameSiteMode.Strict, // prevents CSRF attacks
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            return TypedResults.Ok(acessToken);

        }).AllowAnonymous();

        return app;
    }
}
    