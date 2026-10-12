using System.Text.Json.Serialization;

namespace TaskBoard.Api.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3
}
