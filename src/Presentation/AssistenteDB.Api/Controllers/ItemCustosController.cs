using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
public class ItemCustosController : ControllerBase
{
    private readonly IItemCustoRepository _repo;
    private readonly IItemRepository _itemRepo;

    public ItemCustosController(IItemCustoRepository repo, IItemRepository itemRepo)
    {
        _repo = repo;
        _itemRepo = itemRepo;
    }

    [HttpGet("api/itens/{itemId:long}/custos")]
    public async Task<IActionResult> GetByItem(long itemId)
    {
        var item = await _itemRepo.GetByIdAsync(itemId);
        if (item is null) return NotFound(new { error = new { code = "ITEM_NOT_FOUND", message = "Item não encontrado." } });
        var custos = await _repo.GetByItemIdAsync(itemId);
        return Ok(new { data = custos.Select(ToDto) });
    }

    [HttpGet("api/custos/{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var custo = await _repo.GetByIdAsync(id);
        if (custo is null) return NotFound();
        return Ok(new { data = ToDto(custo) });
    }

    [HttpPost("api/itens/{itemId:long}/custos")]
    public async Task<IActionResult> Create(long itemId, [FromBody] CreateItemCustoDto dto)
    {
        var item = await _itemRepo.GetByIdAsync(itemId);
        if (item is null) return NotFound(new { error = new { code = "ITEM_NOT_FOUND", message = "Item não encontrado." } });

        if (dto.Peso is null && dto.Tempo is null && dto.Quantidade is null && dto.Perdas is null)
            return UnprocessableEntity(new { error = new { code = "CUSTO_SEM_VALORES", message = "Pelo menos um campo de custo deve ser informado." } });

        var custo = new ItemCusto { ItemId = itemId, Peso = dto.Peso, Tempo = dto.Tempo, Quantidade = dto.Quantidade, Perdas = dto.Perdas };
        var created = await _repo.CreateAsync(custo);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { data = ToDto(created) });
    }

    [HttpPut("api/custos/{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateItemCustoDto dto)
    {
        if (dto.Peso is null && dto.Tempo is null && dto.Quantidade is null && dto.Perdas is null)
            return UnprocessableEntity(new { error = new { code = "CUSTO_SEM_VALORES", message = "Pelo menos um campo de custo deve ser informado." } });

        var updated = await _repo.UpdateAsync(id, new ItemCusto { Peso = dto.Peso, Tempo = dto.Tempo, Quantidade = dto.Quantidade, Perdas = dto.Perdas });
        if (updated is null) return NotFound();
        return Ok(new { data = ToDto(updated) });
    }

    [HttpDelete("api/custos/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static ItemCustoResponseDto ToDto(ItemCusto c) =>
        new(c.Id, c.ItemId, c.Peso, c.Tempo, c.Quantidade, c.Perdas);
}
