using Web.Models;
using Bot.Models;
using Bot.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Discord;

[ApiController]
[Route("api/discord/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly QuoteService _quoteService;

    public QuotesController(QuoteService quoteService) =>
        _quoteService = quoteService;
    
    [HttpGet]
    public async Task<List<Quote>> Get() =>
        await _quoteService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Quote>> Get(string id)
    {
        var quote = await _quoteService.GetAsync(id);

        if (quote == null)
            return NotFound();

        return quote;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Quote newBook)
    {
        await _quoteService.CreateAsync(newBook);

        return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Quote updatedBook)
    {
        var quote = await _quoteService.GetAsync(id);

        if (quote is null)
            return NotFound();

        updatedBook.Id = quote.Id;

        await _quoteService.UpdateAsync(id, updatedBook);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _quoteService.GetAsync(id);

        if (book is null)
            return NotFound();

        await _quoteService.RemoveAsync(id);

        return NoContent();
    }
}