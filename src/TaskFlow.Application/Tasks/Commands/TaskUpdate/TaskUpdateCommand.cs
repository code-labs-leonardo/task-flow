using MediatR;
using TaskFlow.Application.Tasks.DTOs;

namespace TaskFlow.Application.Tasks.Commands.TaskUpdate;

public record TaskUpdateCommand(Guid Id, TaskUpdateInput Data) : IRequest<TaskResponse>;
