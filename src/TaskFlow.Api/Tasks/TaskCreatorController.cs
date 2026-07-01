using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks.Commands.TaskCreate;

namespace TaskFlow.Api.Tasks;

[ApiController]
[Route("projetos/{projectId:guid}/tarefas")]
public class TaskCreatorController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskCreatorController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] TaskCreateRequest request, CancellationToken ct)
    {
        var command = new TaskCreateCommand(projectId, new TaskCreateInput(request.Title, request.Description, request.Priority));
        var result = await _mediator.Send(command, ct);
        return CreatedAtRoute("GetTasksByProject", new { projectId = result.ProjectId }, result);
    }
}

public record TaskCreateRequest(string Title, string? Description, string Priority);
