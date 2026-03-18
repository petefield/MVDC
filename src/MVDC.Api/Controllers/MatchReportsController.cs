using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchReportsController : ControllerBase
{
    private readonly IRepository<MatchReport> _repository;

    public MatchReportsController(IRepository<MatchReport> repository) => _repository = repository;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchReport>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _repository.GetAllAsync(cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<MatchReport>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPost]
    public async Task<ActionResult<MatchReport>> Create(MatchReport report, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(report, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPut("{id}")]
    public async Task<ActionResult<MatchReport>> Update(string id, MatchReport report, CancellationToken cancellationToken)
    {
        if (id != report.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, report, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
