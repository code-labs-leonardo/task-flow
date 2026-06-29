using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Application.Tasks.DTOs;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Application.Tasks.Commands.TaskUpdate;

public class TaskUpdateHandler : IRequestHandler<TaskUpdateCommand, TaskResponse>
{
    private readonly ITaskItemRepository _repository;

    public TaskUpdateHandler(ITaskItemRepository repository) => _repository = repository;

    public async Task<TaskResponse> Handle(TaskUpdateCommand request, CancellationToken ct)
    {
        var task = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("TaskItem", request.Id);

        TaskItemStatus? status = request.Data.Status?.ToLowerInvariant() switch
        {
            "pending" => TaskItemStatus.Pending,
            "in_progress" => TaskItemStatus.InProgress,
            "done" => TaskItemStatus.Done,
            _ => null
        };

        TaskPriority? priority = request.Data.Priority is null
            ? null
            : Enum.Parse<TaskPriority>(request.Data.Priority, ignoreCase: true);

        task.Update(request.Data.Title, request.Data.Description, status, priority);
        await _repository.UpdateAsync(task, ct);

        return new(task.Id, task.Title, task.Description,
            task.Status.ToApiString(),
            task.Priority.ToString().ToLowerInvariant(),
            task.CreatedAt, task.CompletedAt, task.ProjectId);
    }
}
