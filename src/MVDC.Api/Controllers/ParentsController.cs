using Microsoft.AspNetCore.Mvc;
using MVDC.Api.Services;
using MVDC.Shared.Models;

namespace MVDC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParentsController : ControllerBase
{
    private readonly IRepository<Parent> _repository;

    public ParentsController(IRepository<Parent> repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parent>>> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Parent>> GetById(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Parent>> Create(Parent parent)
    {
        var created = await _repository.CreateAsync(parent);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Parent>> Update(string id, Parent parent)
    {
        if (id != parent.Id) return BadRequest();
        return Ok(await _repository.UpdateAsync(id, parent));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
