using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid ProjectId { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string? description, TaskPriority priority, Guid projectId)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Status = TaskItemStatus.Pending,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            ProjectId = projectId
        };
    }

    public void Update(string? title, string? description, TaskItemStatus? status, TaskPriority? priority)
    {
        if (title is not null) Title = title;
        if (description is not null) Description = description;
        if (priority.HasValue) Priority = priority.Value;
        if (status.HasValue) TransitionTo(status.Value);
    }

    public bool CanDelete() => Status == TaskItemStatus.Pending;

    private void TransitionTo(TaskItemStatus newStatus)
    {
        var valid = (Status, newStatus) switch
        {
            (TaskItemStatus.Pending, TaskItemStatus.InProgress) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Done) => true,
            _ => false
        };

        if (!valid)
            throw new DomainException(
                $"Transição de '{Status}' para '{newStatus}' não é permitida. " +
                "O fluxo obrigatório é: pending → in_progress → done.");

        Status = newStatus;

        if (newStatus == TaskItemStatus.Done)
            CompletedAt = DateTime.UtcNow;
    }
}
