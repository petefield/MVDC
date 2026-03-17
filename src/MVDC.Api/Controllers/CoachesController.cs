using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CoachesController : ControllerBase
{
    private readonly IRepository<Coach> _repository;

    public CoachesController(IRepository<Coach> repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Coach>>> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Coach>> GetById(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Coach>> Create(Coach coach)
    {
        var created = await _repository.CreateAsync(coach);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Coach>> Update(string id, Coach coach)
    {
        if (id != coach.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, coach));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
