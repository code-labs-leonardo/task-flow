using MediatR;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Application.Tasks.DTOs;
using TaskFlow.Domain.Projects;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Queries.TaskList;

public class TaskListHandler : IRequestHandler<TaskListQuery, PagedResponse<TaskResponse>>
{
    private readonly IProjectRepository _projects;
    private readonly ITaskItemRepository _tasks;

    public TaskListHandler(IProjectRepository projects, ITaskItemRepository tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    public async Task<PagedResponse<TaskResponse>> Handle(TaskListQuery request, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        TaskItemStatus? status = request.Status?.ToLowerInvariant() switch
        {
            "pending" => TaskItemStatus.Pending,
            "in_progress" => TaskItemStatus.InProgress,
            "done" => TaskItemStatus.Done,
            _ => null
        };

        TaskPriority? priority = request.Priority is not null
            && Enum.TryParse<TaskPriority>(request.Priority, ignoreCase: true, out var parsedPriority)
            ? parsedPriority
            : null;

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (tasks, total) = await _tasks.GetByProjectIdAsync(
            project.Id, status, priority, pageNumber, pageSize, ct);

        var items = tasks.Select(t => new TaskResponse(
            t.Id, t.Title, t.Description,
            t.Status.ToApiString(),
            t.Priority.ToString().ToLowerInvariant(),
            t.CreatedAt, t.CompletedAt, t.ProjectId))
            .ToList();

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResponse<TaskResponse>(items, pageNumber, pageSize, total, totalPages);
    }
}
