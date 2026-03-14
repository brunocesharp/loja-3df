using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemRepository _repo;

    public ItemsController(IItemRepository repo)
    {
        _repo = repo;
    }

    // GET api/items
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        return Ok(items);
    }

    // GET api/items/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    // POST api/items
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        var item = new Item
        {
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await _repo.CreateAsync(item);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/items/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemDto dto)
    {
        var updated = await _repo.UpdateAsync(id, new Item
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        });

        return updated is null ? NotFound() : Ok(updated);
    }

    // DELETE api/items/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
