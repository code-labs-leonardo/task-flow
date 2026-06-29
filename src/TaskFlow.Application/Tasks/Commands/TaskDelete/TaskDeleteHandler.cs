using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Domain.Shared;
using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.Commands.TaskDelete;

public class TaskDeleteHandler : IRequestHandler<TaskDeleteCommand>
{
    private readonly ITaskItemRepository _repository;

    public TaskDeleteHandler(ITaskItemRepository repository) => _repository = repository;

    public async Task Handle(TaskDeleteCommand request, CancellationToken ct)
    {
        var task = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("TaskItem", request.Id);

        if (!task.CanDelete())
            throw new DomainException(
                $"Não é possível excluir uma tarefa com status '{task.Status.ToApiString()}'. " +
                "Apenas tarefas com status 'pending' podem ser removidas.");

        await _repository.DeleteAsync(task, ct);
    }
}
