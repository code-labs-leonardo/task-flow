using MediatR;
using TaskFlow.Application.Projects.DTOs;

namespace TaskFlow.Application.Projects.Commands.ProjectCreate;

public record ProjectCreateCommand(string Name, string? Description) : IRequest<ProjectResponse>;
