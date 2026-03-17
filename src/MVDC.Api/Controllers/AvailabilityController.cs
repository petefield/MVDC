using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly IRepository<PlayerAvailability> _repository;

    public AvailabilityController(IRepository<PlayerAvailability> repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerAvailability>>> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerAvailability>> GetById(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<PlayerAvailability>> Create(PlayerAvailability availability)
    {
        var created = await _repository.CreateAsync(availability);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerAvailability>> Update(string id, PlayerAvailability availability)
    {
        if (id != availability.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, availability));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
