using MediatR;
using TaskFlow.Application.Projects.DTOs;

namespace TaskFlow.Application.Projects.Commands.ProjectUpdate;

public record ProjectUpdateCommand(Guid Id, ProjectUpdateInput Data) : IRequest<ProjectResponse>;
