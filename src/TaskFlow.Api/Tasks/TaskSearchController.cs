using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks.Queries.TaskList;

namespace TaskFlow.Api.Tasks;

[ApiController]
[Route("projetos/{projectId:guid}/tarefas")]
public class TaskSearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskSearchController(IMediator mediator) => _mediator = mediator;

    [HttpGet(Name = "GetTasksByProject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(
        Guid projectId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new TaskListQuery(projectId, status, priority, pageNumber, pageSize), ct);
        return Ok(result);
    }
}
