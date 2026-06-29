using MediatR;
using TaskFlow.Application.Projects.DTOs;

namespace TaskFlow.Application.Projects.Queries.ProjectGetById;

public record ProjectGetByIdQuery(Guid Id) : IRequest<ProjectResponse>;
