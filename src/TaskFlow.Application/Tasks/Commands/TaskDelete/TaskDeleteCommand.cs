using MediatR;

namespace TaskFlow.Application.Tasks.Commands.TaskDelete;

public record TaskDeleteCommand(Guid Id) : IRequest;
