using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects.Commands.ProjectCreate;

namespace TaskFlow.Api.Projects;

[ApiController]
[Route("projetos")]
public class ProjectCreatorController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectCreatorController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProjectCreateRequest request, CancellationToken ct)
    {
        var command = new ProjectCreateCommand(request.Name, request.Description);
        var result = await _mediator.Send(command, ct);
        return CreatedAtRoute("GetProjectById", new { id = result.Id }, result);
    }
}

public record ProjectCreateRequest(string Name, string? Description);
