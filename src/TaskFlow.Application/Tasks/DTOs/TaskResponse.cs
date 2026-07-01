namespace TaskFlow.Application.Tasks.DTOs;

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    Guid ProjectId
);
