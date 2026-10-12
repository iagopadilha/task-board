using TaskBoard.Api.Endpoints;

namespace TaskBoard.Api.ExtensionMethods;

public static class EndpointsExthension
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapBoardEndpoints();
        app.MapTaskEndpoints();
    }
}
