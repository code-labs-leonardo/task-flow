using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects.Queries.ProjectGetById;
using TaskFlow.Application.Projects.Queries.ProjectList;

namespace TaskFlow.Api.Projects;

[ApiController]
[Route("projetos")]
public class ProjectSearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectSearchController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ProjectListQuery(status, pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetProjectById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ProjectGetByIdQuery(id), ct);
        return Ok(result);
    }
}
