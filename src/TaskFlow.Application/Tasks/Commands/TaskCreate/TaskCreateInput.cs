namespace TaskFlow.Application.Tasks.Commands.TaskCreate;

public record TaskCreateInput(
    string Title,
    string? Description,
    string Priority);
