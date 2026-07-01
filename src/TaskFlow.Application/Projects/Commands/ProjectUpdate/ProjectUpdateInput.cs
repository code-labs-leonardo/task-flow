namespace TaskFlow.Application.Projects.Commands.ProjectUpdate;

public record ProjectUpdateInput(
    string? Name,
    string? Description,
    string? Status);
