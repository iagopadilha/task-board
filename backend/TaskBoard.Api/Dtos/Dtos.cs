using TaskBoard.Api.Domain;
using TaskBoard.Api.Domain.Enums;

namespace TaskBoard.Api.Dtos;

// ---- Requests ----
public record CreateUserRequest(string Name, string Email, string password);

public record CreateLoginDto(string email, string password);

public record ReturnLogin(string jwtAcessToken);

public record CreateBoardRequest(string Name);

public record CreateTaskRequest(string Title, string? Description, Priority? Priority, DateTime? DueDate);

public record UpdateTaskRequest(string Title, string? Description, Priority Priority, DateTime? DueDate);

public record ChangeStatusRequest(TaskItemStatus Status);

/// <summary>AssigneeId = null removes the assignee.</summary>
public record AssignTaskRequest(Guid? AssigneeId);

public enum TaskSortField { CreatedAt, DueDate, Priority }

/// <summary>Query string of GET /api/boards/{id}/tasks.</summary>
public record TaskListQuery(
    TaskItemStatus? Status = null,
    Guid? AssigneeId = null,
    Priority? Priority = null,
    string? Search = null,
    TaskSortField SortBy = TaskSortField.CreatedAt,
    bool Descending = true,
    int Page = 1,
    int PageSize = 20);

// ---- Responses ----
public record UserResponse(Guid Id, string Name, string Email);

public record BoardResponse(Guid Id, string Name, Guid OwnerId);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    Priority Priority,
    DateTime? DueDate,
    Guid? AssigneeId,
    Guid BoardId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsOverdue);

public record TaskListResponse(
    IReadOnlyList<TaskResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int OverdueCount);

// ---- Mapping ----
public static class Mappings
{
    public static BoardResponse ToResponse(this Board b) => new(b.Id, b.Name, b.OwnerId);

    public static TaskResponse ToResponse(this TaskItem t, DateTime nowUtc) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
        t.AssigneeId, t.BoardId, t.CreatedAt, t.UpdatedAt, t.IsOverdue(nowUtc));
}
