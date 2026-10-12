using System.Text.Json.Serialization;

namespace TaskBoard.Api.Domain.Enums;

// Named TaskItemStatus (not TaskStatus) to avoid clashing with System.Threading.Tasks.TaskStatus.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskItemStatus
{
    Todo = 1,
    InProgress = 2,
    Review = 3,
    Done = 4
}
