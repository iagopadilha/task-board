using TaskBoard.Api.Data;
using TaskBoard.Api.Endpoints;
using TaskBoard.Api.Middleware;

namespace TaskBoard.Api.ExtensionMethods;

public static class ApiConfigExthension
{
    public static void AddPreApiConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDatabase(builder.Configuration); // database exthension method here

        builder.Services.AddDependencyInjectionConfig();
    }

    public static void AddPosApiConfig(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // TODO(auth): UseAuthentication / UseAuthorization go here, before the Map* calls.

        app.MapEndpoints(); // endpoints routing
  
    }
}
