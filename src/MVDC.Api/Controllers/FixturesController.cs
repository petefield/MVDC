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
    public async Task<ActionResult<IEnumerable<Fixture>>> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Fixture>> GetById(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshFromFullTime()
    {
        await FullTimeFixtureSeeder.SeedAsync(_repository, _logger);
        var fixtures = await _repository.GetAllAsync();
        return Ok(fixtures);
    }

    [HttpPost]
    public async Task<ActionResult<Fixture>> Create(Fixture fixture)
    {
        var created = await _repository.CreateAsync(fixture);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Fixture>> Update(string id, Fixture fixture)
    {
        if (id != fixture.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, fixture));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
