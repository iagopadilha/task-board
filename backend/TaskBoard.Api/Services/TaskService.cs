using TaskBoard.Api.Domain;
using TaskBoard.Api.Domain.Enums;
using TaskBoard.Api.Domain.Exceptions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Api.Services;

public interface ITaskService
{
    Task<TaskListResponse> ListAsync(Guid boardId, Guid userId, TaskListQuery query);
    Task<TaskResponse> CreateAsync(Guid boardId, Guid userId, CreateTaskRequest request);
    Task<TaskResponse> UpdateAsync(Guid taskId, Guid userId, UpdateTaskRequest request);
    Task<TaskResponse> ChangeStatusAsync(Guid taskId, Guid userId, ChangeStatusRequest request);
    Task<TaskResponse> AssignAsync(Guid taskId, Guid userId, AssignTaskRequest request);
    Task DeleteAsync(Guid taskId, Guid userId);
}

/// <summary>
/// Orchestrates use cases: loads data, checks permissions and rules that need the database
/// (board ownership, WIP limit, assignee exists) and delegates the rest to <see cref="TaskItem"/>.
/// </summary>
public class TaskService(
    ITaskRepository tasks,
    IBoardRepository boards,
    IUserRepository users,
    TimeProvider clock) : ITaskService
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<TaskListResponse> ListAsync(Guid boardId, Guid userId, TaskListQuery query)
    {
        var board = await GetBoardAsync(boardId);
        if (board.OwnerId != userId && !await tasks.HasAssignedTaskAsync(boardId, userId))
            throw new ForbiddenException("You do not have access to this board.");

        var normalized = query with { Page = Math.Max(1, query.Page), PageSize = Math.Clamp(query.PageSize, 1, 100) };
        var now = Now;

        var (items, total) = await tasks.QueryAsync(boardId, normalized);
        var overdue = await tasks.CountOverdueAsync(boardId, now.Date);

        return new TaskListResponse(
            items.Select(t => t.ToResponse(now)).ToList(), normalized.Page, normalized.PageSize, total, overdue);
    }

    public async Task<TaskResponse> CreateAsync(Guid boardId, Guid userId, CreateTaskRequest request)
    {
        var board = await GetBoardAsync(boardId);
        if (board.OwnerId != userId)
            throw new ForbiddenException("Only the board owner can create tasks.");

        var now = Now;
        var task = TaskItem.Create(boardId, request.Title, request.Description, request.Priority, request.DueDate, now);
        await tasks.AddAsync(task);
        return task.ToResponse(now);
    }

    public async Task<TaskResponse> UpdateAsync(Guid taskId, Guid userId, UpdateTaskRequest request)
    {
        var task = await LoadForModificationAsync(taskId, userId);

        var now = Now;
        task.Update(request.Title, request.Description, request.Priority, request.DueDate, now);
        await tasks.UpdateAsync(task);
        return task.ToResponse(now);
    }

    public async Task<TaskResponse> ChangeStatusAsync(Guid taskId, Guid userId, ChangeStatusRequest request)
    {
        var task = await LoadForModificationAsync(taskId, userId);

        task.EnsureCanMoveTo(request.Status); // rejects invalid transitions before hitting the database
        if (request.Status == TaskItemStatus.InProgress)
            await EnsureWipLimitAsync(task.AssigneeId!.Value, task.Id);

        var now = Now;
        task.MoveTo(request.Status, now);
        await tasks.UpdateAsync(task);
        return task.ToResponse(now);
    }

    public async Task<TaskResponse> AssignAsync(Guid taskId, Guid userId, AssignTaskRequest request)
    {
        var task = await LoadForModificationAsync(taskId, userId);

        if (request.AssigneeId is { } assigneeId)
        {
            if (!await users.ExistsAsync(assigneeId))
                throw new DomainValidationException("Assignee does not exist.");
            if (task.Status == TaskItemStatus.InProgress && assigneeId != task.AssigneeId)
                await EnsureWipLimitAsync(assigneeId, task.Id);
        }

        var now = Now;
        task.Assign(request.AssigneeId, now);
        await tasks.UpdateAsync(task);
        return task.ToResponse(now);
    }

    public async Task DeleteAsync(Guid taskId, Guid userId)
    {
        var task = await GetTaskAsync(taskId);
        var board = await GetBoardAsync(task.BoardId);
        if (board.OwnerId != userId)
            throw new ForbiddenException("Only the board owner can delete tasks.");

        task.EnsureCanBeDeleted();
        await tasks.DeleteAsync(task);
    }

    // ---- helpers ----

    private async Task<Board> GetBoardAsync(Guid boardId) =>
        await boards.GetByIdAsync(boardId) ?? throw new NotFoundException("Board not found.");

    private async Task<TaskItem> GetTaskAsync(Guid taskId) =>
        await tasks.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");

    /// <summary>Edit / move / assign: only the board owner or the task assignee.</summary>
    private async Task<TaskItem> LoadForModificationAsync(Guid taskId, Guid userId)
    {
        var task = await GetTaskAsync(taskId);
        var board = await GetBoardAsync(task.BoardId);
        if (board.OwnerId != userId && task.AssigneeId != userId)
            throw new ForbiddenException("Only the board owner or the assignee can modify this task.");
        return task;
    }

    private async Task EnsureWipLimitAsync(Guid assigneeId, Guid taskId)
    {
        var current = await tasks.CountInProgressAsync(assigneeId, excludeTaskId: taskId);
        if (current >= TaskItem.MaxInProgressPerAssignee)
            throw new BusinessRuleException(
                $"Assignee already has {TaskItem.MaxInProgressPerAssignee} tasks in progress (WIP limit).");
    }
}
