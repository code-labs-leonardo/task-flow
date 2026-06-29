using MediatR;
using TaskFlow.Application.Tasks.DTOs;

namespace TaskFlow.Application.Tasks.Commands.TaskCreate;

public record TaskCreateCommand(Guid ProjectId, TaskCreateInput Data) : IRequest<TaskResponse>;
