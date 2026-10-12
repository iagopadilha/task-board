using TaskBoard.Api.Domain.Enums;
using TaskBoard.Api.Domain.Exceptions;

namespace TaskBoard.Api.Domain;

/// <summary>
/// Task aggregate. Holds the state machine and the rules that do not need the database.
/// "Now" is always passed in (UTC) so the rules stay deterministic and testable.
/// DueDate has day granularity: a task due today is neither invalid nor overdue until tomorrow.
/// </summary>
public class TaskItem
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 100;
    public const int DescriptionMaxLength = 2000;
    public const int MaxInProgressPerAssignee = 5;

    private static readonly Dictionary<TaskItemStatus, TaskItemStatus[]> AllowedTransitions = new()
    {
        [TaskItemStatus.Todo] = [TaskItemStatus.InProgress],
        [TaskItemStatus.InProgress] = [TaskItemStatus.Review],
        [TaskItemStatus.Review] = [TaskItemStatus.Done, TaskItemStatus.InProgress],
        [TaskItemStatus.Done] = []
    };

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public Priority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid BoardId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(
        Guid boardId, string title, string? description, Priority? priority, DateTime? dueDate, DateTime nowUtc)
    {
        var due = NormalizeDueDate(dueDate);
        if (due is { } d && d.Date < nowUtc.Date)
            throw new DomainValidationException("Due date cannot be in the past.");

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            Title = ValidateTitle(title),
            Description = ValidateDescription(description),
            Status = TaskItemStatus.Todo,
            Priority = priority ?? Priority.Medium,
            DueDate = due,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };
    }

    public void Update(string title, string? description, Priority priority, DateTime? dueDate, DateTime nowUtc)
    {
        EnsureNotDone();

        var due = NormalizeDueDate(dueDate);
        // Only a *changed* due date is validated, so an overdue task can still be edited.
        if (due != DueDate && due is { } d && d.Date < nowUtc.Date)
            throw new DomainValidationException("Due date cannot be in the past.");

        Title = ValidateTitle(title);
        Description = ValidateDescription(description);
        Priority = priority;
        DueDate = due;
        UpdatedAt = nowUtc;
    }

    public void Assign(Guid? assigneeId, DateTime nowUtc)
    {
        EnsureNotDone();
        if (assigneeId is null && Status == TaskItemStatus.InProgress)
            throw new BusinessRuleException("An in-progress task must have an assignee.");

        AssigneeId = assigneeId;
        UpdatedAt = nowUtc;
    }

    /// <summary>Pure validation of a status change. The WIP limit lives in the service (needs the database).</summary>
    public void EnsureCanMoveTo(TaskItemStatus target)
    {
        EnsureNotDone();

        if (!AllowedTransitions[Status].Contains(target))
            throw new BusinessRuleException($"Cannot move a task from {Status} to {target}.");
        if (target == TaskItemStatus.InProgress && AssigneeId is null)
            throw new BusinessRuleException("A task needs an assignee before moving to InProgress.");
    }

    public void MoveTo(TaskItemStatus target, DateTime nowUtc)
    {
        EnsureCanMoveTo(target);
        Status = target;
        UpdatedAt = nowUtc;
    }

    public void EnsureCanBeDeleted()
    {
        if (Status == TaskItemStatus.InProgress)
            throw new BusinessRuleException("An in-progress task cannot be deleted. Move it back to Todo first.");
    }

    public bool IsOverdue(DateTime nowUtc) =>
        Status != TaskItemStatus.Done && DueDate is { } d && d.Date < nowUtc.Date;

    private void EnsureNotDone()
    {
        if (Status == TaskItemStatus.Done)
            throw new BusinessRuleException("A done task is immutable.");
    }

    private static string ValidateTitle(string? title)
    {
        title = title?.Trim() ?? string.Empty;
        if (title.Length is < TitleMinLength or > TitleMaxLength)
            throw new DomainValidationException(
                $"Title is required and must have between {TitleMinLength} and {TitleMaxLength} characters.");
        return title;
    }

    private static string? ValidateDescription(string? description)
    {
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (description is { Length: > DescriptionMaxLength })
            throw new DomainValidationException($"Description must have at most {DescriptionMaxLength} characters.");
        return description;
    }

    // Npgsql timestamptz only accepts UTC values.
    private static DateTime? NormalizeDueDate(DateTime? value) => value switch
    {
        null => null,
        { Kind: DateTimeKind.Utc } v => v,
        { Kind: DateTimeKind.Local } v => v.ToUniversalTime(),
        var v => DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
    };
}
