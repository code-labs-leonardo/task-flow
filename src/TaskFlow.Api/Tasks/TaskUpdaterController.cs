using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks.Commands.TaskUpdate;

namespace TaskFlow.Api.Tasks;

[ApiController]
[Route("tarefas")]
public class TaskUpdaterController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskUpdaterController(IMediator mediator) => _mediator = mediator;

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskUpdateRequest request, CancellationToken ct)
    {
        var command = new TaskUpdateCommand(id, new TaskUpdateInput(request.Title, request.Description, request.Status, request.Priority, request.CompletedAt));
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public record TaskUpdateRequest(string? Title, string? Description, string? Status, string? Priority, string? CompletedAt);
