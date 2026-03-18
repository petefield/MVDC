using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FixturesController : ControllerBase
{
    private readonly IRepository<Fixture> _repository;
    private readonly ILogger<FixturesController> _logger;

    public FixturesController(IRepository<Fixture> repository, ILogger<FixturesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fixture>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _repository.GetAllAsync(cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Fixture>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshFromFullTime(CancellationToken cancellationToken)
    {
        await FullTimeFixtureSeeder.SeedAsync(_repository, _logger, cancellationToken);
        var fixtures = await _repository.GetAllAsync(cancellationToken);
        return Ok(fixtures);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPost]
    public async Task<ActionResult<Fixture>> Create(Fixture fixture, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(fixture, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Fixture>> Update(string id, Fixture fixture, CancellationToken cancellationToken)
    {
        if (id != fixture.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, fixture, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
