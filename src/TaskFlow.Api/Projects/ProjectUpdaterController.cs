using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects.Commands.ProjectUpdate;

namespace TaskFlow.Api.Projects;

[ApiController]
[Route("projetos")]
public class ProjectUpdaterController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectUpdaterController(IMediator mediator) => _mediator = mediator;

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProjectUpdateRequest request, CancellationToken ct)
    {
        var command = new ProjectUpdateCommand(id, new ProjectUpdateInput(request.Name, request.Description, request.Status));
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public record ProjectUpdateRequest(string? Name, string? Description, string? Status);
