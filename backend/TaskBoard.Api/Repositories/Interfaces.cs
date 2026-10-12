using TaskBoard.Api.Domain;
using TaskBoard.Api.Dtos;

namespace TaskBoard.Api.Repositories;

// Thin repositories: one method per use, each write persists immediately (SaveChanges).
// Kept as interfaces so services can be unit tested with mocks.

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(User user);
}

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(Guid id);
    /// <summary>Boards the user owns or has at least one task assigned in.</summary>
    Task<IReadOnlyList<Board>> ListForUserAsync(Guid userId);
    Task AddAsync(Board board);
}

public interface ITaskRepository
{
    /// <summary>Tracked entity, ready to be modified and saved with <see cref="UpdateAsync"/>.</summary>
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> QueryAsync(Guid boardId, TaskListQuery query);
    /// <param name="todayUtc">UTC midnight of today. Overdue = DueDate before it and not Done.</param>
    Task<int> CountOverdueAsync(Guid boardId, DateTime todayUtc);
    Task<int> CountInProgressAsync(Guid assigneeId, Guid? excludeTaskId = null);
    Task<bool> HasAssignedTaskAsync(Guid boardId, Guid userId);
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}
