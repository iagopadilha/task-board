using Microsoft.OpenApi.Models;
using TaskBoard.Api.Data;
using TaskBoard.Api.Middleware;
using TaskBoard.Api.Security;

namespace TaskBoard.Api.ExtensionMethods;

public static class ApiConfigExthension
{
    public static void AddPreApiConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Placeholder auth: lets Swagger UI send the X-User-Id header on every request (Authorize button).
            options.AddSecurityDefinition("UserId", new OpenApiSecurityScheme
            {
                Name = HeaderCurrentUser.HeaderName,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "Id (Guid) of the calling user."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "UserId" }
                }] = []
            });
        });

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
