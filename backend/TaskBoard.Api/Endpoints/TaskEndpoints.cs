using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Security;
using TaskBoard.Api.Services;

namespace TaskBoard.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app) // Using TypedResults for strongly-typed responses and automatic Swagger documentation
    {

        var api = app.MapGroup("/api").WithTags("Tasks");

        api.MapGet("/boards/{boardId:guid}/tasks",
            async (Guid boardId, [AsParameters] TaskListQuery query, ICurrentUser user, ITaskService tasks) =>
                TypedResults.Ok(await tasks.ListAsync(boardId, user.UserId, query)));

        api.MapPost("/boards/{boardId:guid}/tasks",
            async (Guid boardId, CreateTaskRequest request, ICurrentUser user, ITaskService tasks) =>
            {
                var task = await tasks.CreateAsync(boardId, user.UserId, request);
                return TypedResults.Created($"/api/tasks/{task.Id}", task);
            });

        api.MapPut("/tasks/{id:guid}",
            async (Guid id, UpdateTaskRequest request, ICurrentUser user, ITaskService tasks) =>
                TypedResults.Ok(await tasks.UpdateAsync(id, user.UserId, request)));

        api.MapPatch("/tasks/{id:guid}/status",
            async (Guid id, ChangeStatusRequest request, ICurrentUser user, ITaskService tasks) =>
                TypedResults.Ok(await tasks.ChangeStatusAsync(id, user.UserId, request)));

        api.MapPatch("/tasks/{id:guid}/assignee",
            async (Guid id, AssignTaskRequest request, ICurrentUser user, ITaskService tasks) =>
                TypedResults.Ok(await tasks.AssignAsync(id, user.UserId, request)));

        api.MapDelete("/tasks/{id:guid}",
            async (Guid id, ICurrentUser user, ITaskService tasks) =>
            {
                await tasks.DeleteAsync(id, user.UserId);
                return TypedResults.NoContent();
            });

        return app;
    }
}
