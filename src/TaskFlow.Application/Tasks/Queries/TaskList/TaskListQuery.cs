using MediatR;
using TaskFlow.Application.Common.DTOs;
using TaskFlow.Application.Tasks.DTOs;

namespace TaskFlow.Application.Tasks.Queries.TaskList;

public record TaskListQuery(
    Guid ProjectId,
    string? Status,
    string? Priority,
    int PageNumber = 1,
    int PageSize = 100) : IRequest<PagedResponse<TaskResponse>>;
