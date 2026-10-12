using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Data;
using TaskBoard.Api.Domain;
using TaskBoard.Api.Domain.Enums;
using TaskBoard.Api.Dtos;

namespace TaskBoard.Api.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());

    public Task<bool> ExistsAsync(Guid id) => db.Users.AnyAsync(u => u.Id == id);

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}

public class BoardRepository(AppDbContext db) : IBoardRepository
{
    public Task<Board?> GetByIdAsync(Guid id) => db.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<Board>> ListForUserAsync(Guid userId) =>
        await db.Boards.AsNoTracking()
            .Where(b => b.OwnerId == userId || db.Tasks.Any(t => t.BoardId == b.Id && t.AssigneeId == userId))
            .OrderBy(b => b.Name)
            .ToListAsync();

    public async Task AddAsync(Board board)
    {
        db.Boards.Add(board);
        await db.SaveChangesAsync();
    }
}

public class TaskRepository(AppDbContext db) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id) => db.Tasks.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> QueryAsync(Guid boardId, TaskListQuery q)
    {
        var query = db.Tasks.AsNoTracking().Where(t => t.BoardId == boardId);

        if (q.Status is { } status) query = query.Where(t => t.Status == status);
        if (q.AssigneeId is { } assignee) query = query.Where(t => t.AssigneeId == assignee);
        if (q.Priority is { } priority) query = query.Where(t => t.Priority == priority);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(term));
        }

        var total = await query.CountAsync();

        var ordered = (q.SortBy, q.Descending) switch
        {
            (TaskSortField.DueDate, false) => query.OrderBy(t => t.DueDate),
            (TaskSortField.DueDate, true) => query.OrderByDescending(t => t.DueDate),
            (TaskSortField.Priority, false) => query.OrderBy(t => t.Priority),
            (TaskSortField.Priority, true) => query.OrderByDescending(t => t.Priority),
            (_, false) => query.OrderBy(t => t.CreatedAt),
            (_, true) => query.OrderByDescending(t => t.CreatedAt)
        };

        var items = await ordered
            .ThenBy(t => t.Id) // deterministic pagination when the sort key ties
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<int> CountOverdueAsync(Guid boardId, DateTime todayUtc) =>
        db.Tasks.CountAsync(t =>
            t.BoardId == boardId && t.Status != TaskItemStatus.Done && t.DueDate != null && t.DueDate < todayUtc);

    public Task<int> CountInProgressAsync(Guid assigneeId, Guid? excludeTaskId = null) =>
        db.Tasks.CountAsync(t =>
            t.AssigneeId == assigneeId && t.Status == TaskItemStatus.InProgress && t.Id != excludeTaskId);

    public Task<bool> HasAssignedTaskAsync(Guid boardId, Guid userId) =>
        db.Tasks.AnyAsync(t => t.BoardId == boardId && t.AssigneeId == userId);

    public async Task AddAsync(TaskItem task)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(TaskItem task) => db.SaveChangesAsync();

    public async Task DeleteAsync(TaskItem task)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
    }
}
