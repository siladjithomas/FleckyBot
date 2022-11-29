using Web.Models;
using Bot.Models;
using Bot.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Discord;

[ApiController]
[Route("api/discord/[controller]")]
public class TicketsController : ControllerBase
{
    public readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService) =>
        _ticketService = ticketService;

    [HttpGet]
    public async Task<List<Ticket>> Get() =>
        await _ticketService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Ticket>> Get(string id)
    {
        var ticket = await _ticketService.GetAsync(id);

        if (ticket == null)
            return NotFound();

        return ticket;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Ticket newTicket)
    {
        await _ticketService.CreateAsync(newTicket);

        return CreatedAtAction(nameof(Get), new { id = newTicket.Id }, newTicket);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> update(string id, Ticket updatedTicket)
    {
        var ticket = await _ticketService.GetAsync(id);

        if (ticket is null)
            return NotFound();

        updatedTicket.Id = ticket.Id;

        await _ticketService.UpdateAsync(id, updatedTicket);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var ticket = await _ticketService.GetAsync(id);

        if (ticket is null)
            return NotFound();

        await _ticketService.RemoveAsync(id);

        return NoContent();
    }
}