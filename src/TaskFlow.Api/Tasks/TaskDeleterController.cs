using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks.Commands.TaskDelete;

namespace TaskFlow.Api.Tasks;

[ApiController]
[Route("tarefas")]
public class TaskDeleterController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskDeleterController(IMediator mediator) => _mediator = mediator;

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new TaskDeleteCommand(id), ct);
        return NoContent();
    }
}
