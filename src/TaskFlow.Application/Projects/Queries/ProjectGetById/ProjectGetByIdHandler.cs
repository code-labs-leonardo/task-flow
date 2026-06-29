using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Projects.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Application.Projects.Queries.ProjectGetById;

public class ProjectGetByIdHandler : IRequestHandler<ProjectGetByIdQuery, ProjectResponse>
{
    private readonly IProjectRepository _repository;

    public ProjectGetByIdHandler(IProjectRepository repository) => _repository = repository;

    public async Task<ProjectResponse> Handle(ProjectGetByIdQuery request, CancellationToken ct)
    {
        var project = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        return new(project.Id, project.Name, project.Description,
            project.Status.ToString().ToLowerInvariant(), project.CreatedAt);
    }
}
