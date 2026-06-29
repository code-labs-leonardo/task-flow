using MediatR;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Projects.DTOs;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Application.Projects.Queries.ProjectList;

public class ProjectListHandler : IRequestHandler<ProjectListQuery, PagedResponse<ProjectResponse>>
{
    private readonly IProjectRepository _repository;

    public ProjectListHandler(IProjectRepository repository) => _repository = repository;

    public async Task<PagedResponse<ProjectResponse>> Handle(ProjectListQuery request, CancellationToken ct)
    {
        ProjectStatus? status = request.Status?.ToLowerInvariant() switch
        {
            "active" => ProjectStatus.Active,
            "archived" => ProjectStatus.Archived,
            _ => null
        };

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (projects, total) = await _repository.GetAllAsync(status, pageNumber, pageSize, ct);

        var items = projects
            .Select(p => new ProjectResponse(p.Id, p.Name, p.Description,
                p.Status.ToString().ToLowerInvariant(), p.CreatedAt))
            .ToList();

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResponse<ProjectResponse>(items, pageNumber, pageSize, total, totalPages);
    }
}
