using TaskBoard.Api.Repositories;
using TaskBoard.Api.Security;
using TaskBoard.Api.Services;

namespace TaskBoard.Api.ExtensionMethods;

public static class DependencyInjectionConfigExthension
{
    public static void AddDependencyInjectionConfig(this IServiceCollection serviceConfig)
    {

        // PLACEHOLDER: swap for a claims-based ICurrentUser when auth is added.
        serviceConfig.AddScoped<ICurrentUser, HeaderCurrentUser>();

        serviceConfig.AddScoped<IUserRepository, UserRepository>();
        serviceConfig.AddScoped<IBoardRepository, BoardRepository>();
        serviceConfig.AddScoped<ITaskRepository, TaskRepository>();
        serviceConfig.AddScoped<IUserService, UserService>();
        serviceConfig.AddScoped<IBoardService, BoardService>();
        serviceConfig.AddScoped<ITaskService, TaskService>();
    }
}
