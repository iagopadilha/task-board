using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskBoard.Api.Dtos;

namespace TaskBoard.Api.ExtensionMethods;

public static class AuthConfigExthension
{
    public static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection); // config to globally acess this T - variable
        // this way here we get the required section. Configures it globally typing it

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(config =>
            {
                var jwtSettings = jwtSection.Get<JwtSettings>() // then we get the section typing it
                    ?? throw new InvalidOperationException("JwtSettings is not configured.");

                config.RequireHttpsMetadata = false;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SignInKey
                    ?? throw new InvalidOperationException("Error trying to validate JWT Token!"))),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };

                config.Events = new JwtBearerEvents // Runs first than the JWT token validation - They are Pre and Post Events we have
                {
                    OnMessageReceived = context =>
                    {
                        if (context.HttpContext.Request.Cookies.TryGetValue("acess_token", out var jwtCookieDoesExist))
                        {
                            context.Token = jwtCookieDoesExist;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = failed =>
                    {
                        if (failed.Exception is SecurityTokenExpiredException)
                        {
                            failed.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            // Future schemas can be chained right here (e.g. .AddJwtBearer("SecondSchema", config => ...))

        services.AddAuthorizationBuilder() // default fallback for [Authorize] in every controller
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser() //.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) // <-- Use this to add multiple authentication schemes
            .Build());

        return services;
    }
}
