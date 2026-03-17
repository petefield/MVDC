using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly IRepository<Team> _repository;

    public TeamsController(IRepository<Team> repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetById(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Team>> Create(Team team)
    {
        var created = await _repository.CreateAsync(team);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Team>> Update(string id, Team team)
    {
        if (id != team.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, team));
    }

    [HttpPut("{id}/players")]
    public async Task<ActionResult<Team>> AssignPlayers(string id, [FromBody] List<string> playerIds)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team is null) return NotFound();
        team.PlayerIds = playerIds;
        return Ok(await _repository.UpdateAsync(id, team));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
