using ItemsTrabajo.Application.Commands.AssignWorkItem;
using ItemsTrabajo.Application.Commands.CreateWorkItem;
using ItemsTrabajo.Application.DTOs.WorkItem;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ItemsTrabajo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WorkItemsController : ControllerBase
{
    
    private readonly IMediator _mediator;
    private readonly ISender _sender;

    public WorkItemsController(IMediator mediator, ISender sender)
    {
        _mediator = mediator;
        _sender = sender;
    }
    
    /// <summary>
    /// Crea un nuevo ítem de trabajo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create( [FromBody] CreateWorkItemDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CreateWorkItemCommand(dto), cancellationToken);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    
    [HttpPost("assign")]
    [ProducesResponseType(typeof(AssignWorkItemResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignWorkItemCommand(), cancellationToken);
        return Ok(result);
    }
    
}