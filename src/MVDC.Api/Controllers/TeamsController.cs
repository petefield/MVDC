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
    public async Task<ActionResult<IEnumerable<Team>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _repository.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPost]
    public async Task<ActionResult<Team>> Create(Team team, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(team, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Team>> Update(string id, Team team, CancellationToken cancellationToken)
    {
        if (id != team.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, team, cancellationToken));
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPut("{id}/players")]
    public async Task<ActionResult<Team>> AssignPlayers(string id, [FromBody] List<string> playerIds, CancellationToken cancellationToken)
    {
        var team = await _repository.GetByIdAsync(id, cancellationToken);
        if (team is null) return NotFound();
        team.PlayerIds = playerIds;
        return Ok(await _repository.UpdateAsync(id, team, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
