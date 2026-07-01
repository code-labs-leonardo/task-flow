using MediatR;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Projects.DTOs;

namespace TaskFlow.Application.Projects.Queries.ProjectList;

public record ProjectListQuery(
    string? Status,
    int PageNumber = 1,
    int PageSize = 100) : IRequest<PagedResponse<ProjectResponse>>;
