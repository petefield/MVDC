using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ParentsController : ControllerBase
{
    private readonly IRepository<Parent> _repository;

    public ParentsController(IRepository<Parent> repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parent>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _repository.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<Parent>> GetById(string id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPost]
    public async Task<ActionResult<Parent>> Create(Parent parent, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(parent, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Coach")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Parent>> Update(string id, Parent parent, CancellationToken cancellationToken)
    {
        if (id != parent.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, parent, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
