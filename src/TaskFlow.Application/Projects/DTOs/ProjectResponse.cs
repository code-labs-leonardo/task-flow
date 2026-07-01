namespace TaskFlow.Application.Projects.DTOs;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTime CreatedAt
);
