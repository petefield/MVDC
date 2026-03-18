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
    public async Task<ActionResult<IEnumerable<Coach>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _repository.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<Coach>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPost]
    public async Task<ActionResult<Coach>> Create(Coach coach, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(coach, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Coach>> Update(string id, Coach coach, CancellationToken cancellationToken)
    {
        if (id != coach.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, coach, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
