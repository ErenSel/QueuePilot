using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueuePilot.Application.Tickets.Commands;
using QueuePilot.Application.Tickets.Queries;
using QueuePilot.Domain.Enums;
using System.Security.Claims;

namespace QueuePilot.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketDto dto)
    {
        // For MVP, simplistic user ID extraction. Real world: use a CurrentUserService.
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) 
            return Unauthorized();
            
        // Assuming Customer creates ticket for themselves
        var command = new CreateTicketCommand(userId, dto.Title, dto.Description);
        var ticketId = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetById), new { id = ticketId }, new { id = ticketId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Placeholder for GetById Query
        return Ok(new { Id = id }); 
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] TicketStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetTicketsQuery(page, pageSize, status, search, fromDate, toDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "AgentOnly")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var command = new UpdateTicketStatusCommand(id, dto.Status);
        await _mediator.Send(command);
        return NoContent();
    }
    
    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) 
            return Unauthorized();
            
        var command = new AddTicketCommentCommand(id, userId, dto.Text);
        await _mediator.Send(command); // Note: Handler should return CommentId ideally
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}

public record CreateTicketDto(string Title, string Description);
public record UpdateStatusDto(TicketStatus Status);
public record AddCommentDto(string Text);
