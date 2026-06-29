using MediatR;
using TaskFlow.Application.Projects.DTOs;
using TaskFlow.Domain.Projects;

namespace TaskFlow.Application.Projects.Commands.ProjectCreate;

public class ProjectCreateHandler : IRequestHandler<ProjectCreateCommand, ProjectResponse>
{
    private readonly IProjectRepository _repository;

    public ProjectCreateHandler(IProjectRepository repository) => _repository = repository;

    public async Task<ProjectResponse> Handle(ProjectCreateCommand request, CancellationToken ct)
    {
        var project = Project.Create(request.Name, request.Description);
        await _repository.AddAsync(project, ct);
        return ToResponse(project);
    }

    internal static ProjectResponse ToResponse(Project p) =>
        new(p.Id, p.Name, p.Description, p.Status.ToString().ToLowerInvariant(), p.CreatedAt);
}
