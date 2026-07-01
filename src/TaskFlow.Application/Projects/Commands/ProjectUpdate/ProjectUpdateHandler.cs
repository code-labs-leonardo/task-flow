using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Projects.DTOs;
using TaskFlow.Domain.Projects;
using TaskFlow.Domain.Shared;

namespace TaskFlow.Application.Projects.Commands.ProjectUpdate;

public class ProjectUpdateHandler : IRequestHandler<ProjectUpdateCommand, ProjectResponse>
{
    private readonly IProjectRepository _repository;

    public ProjectUpdateHandler(IProjectRepository repository) => _repository = repository;

    public async Task<ProjectResponse> Handle(ProjectUpdateCommand request, CancellationToken ct)
    {
        var project = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        if (request.Data.Name is not null || request.Data.Description is not null)
            project.Update(request.Data.Name, request.Data.Description);

        if (request.Data.Status?.ToLowerInvariant() == "archived")
        {
            var hasInProgress = await _repository.HasInProgressTasksAsync(project.Id, ct);
            if (hasInProgress)
                throw new DomainException(
                    "Não é possível arquivar um projeto com tarefas em andamento.");

            project.Archive();
        }

        if (request.Data.Status?.ToLowerInvariant() == "active")
            project.Activate();

        await _repository.UpdateAsync(project, ct);

        return new(project.Id, project.Name, project.Description,
            project.Status.ToString().ToLowerInvariant(), project.CreatedAt);
    }
}
