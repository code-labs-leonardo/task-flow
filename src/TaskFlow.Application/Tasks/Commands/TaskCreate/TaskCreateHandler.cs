using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Application.Tasks.DTOs;
using TaskFlow.Domain.Projects;
using TaskFlow.Domain.Shared;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Commands.TaskCreate;

public class TaskCreateHandler : IRequestHandler<TaskCreateCommand, TaskResponse>
{
    private readonly IProjectRepository _projects;
    private readonly ITaskItemRepository _tasks;

    public TaskCreateHandler(IProjectRepository projects, ITaskItemRepository tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    public async Task<TaskResponse> Handle(TaskCreateCommand request, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        if (project.Status == ProjectStatus.Archived)
            throw new DomainException("Não é permitido criar tarefas em um projeto arquivado.");

        var priority = Enum.Parse<TaskPriority>(request.Data.Priority, ignoreCase: true);
        var task = TaskItem.Create(request.Data.Title, request.Data.Description, priority, request.ProjectId);

        await _tasks.AddAsync(task, ct);
        return ToResponse(task);
    }

    internal static TaskResponse ToResponse(TaskItem t) =>
        new(t.Id, t.Title, t.Description,
            t.Status.ToApiString(),
            t.Priority.ToString().ToLowerInvariant(),
            t.CreatedAt, t.CompletedAt, t.ProjectId);
}
