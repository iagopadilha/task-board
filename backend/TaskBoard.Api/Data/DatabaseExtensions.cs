using Microsoft.EntityFrameworkCore;

namespace TaskBoard.Api.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Default") ?? throw new Exception("Connection string is not availiable!");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
