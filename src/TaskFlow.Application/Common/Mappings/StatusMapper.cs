using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.Mappings;

internal static class StatusMapper
{
    internal static string ToApiString(this TaskItemStatus status) => status switch
    {
        TaskItemStatus.InProgress => "in_progress",
        _ => status.ToString().ToLowerInvariant()
    };
}
