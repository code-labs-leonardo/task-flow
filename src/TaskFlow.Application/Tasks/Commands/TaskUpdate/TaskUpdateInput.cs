namespace TaskFlow.Application.Tasks.Commands.TaskUpdate;

public record TaskUpdateInput(
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    string? CompletedAt);
