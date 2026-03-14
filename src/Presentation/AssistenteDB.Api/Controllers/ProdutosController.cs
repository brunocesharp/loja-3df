using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoRepository _repo;
    private readonly ITipoProdutoRepository _tipoRepo;

    public ProdutosController(IProdutoRepository repo, ITipoProdutoRepository tipoRepo)
    {
        _repo = repo;
        _tipoRepo = tipoRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] bool? ativado = null, [FromQuery] long? tipoProdutoId = null)
    {
        pageSize = Math.Min(pageSize, 100);
        var (items, total) = await _repo.GetAllAsync(page, pageSize, ativado, tipoProdutoId);
        return Ok(new
        {
            data = items.Select(ToDto),
            pagination = new { page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize), totalItems = total }
        });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var produto = await _repo.GetByIdAsync(id);
        if (produto is null) return NotFound();
        return Ok(new { data = ToDto(produto) });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProdutoDto dto)
    {
        var tipo = await _tipoRepo.GetByIdAsync(dto.TipoProdutoId);
        if (tipo is null)
            return UnprocessableEntity(new { error = new { code = "TIPO_PRODUTO_NOT_FOUND", message = "tipoProdutoId inválido." } });

        var produto = new Produto { TipoProdutoId = dto.TipoProdutoId, Nome = dto.Nome, Descricao = dto.Descricao, Ativado = dto.Ativado };
        var created = await _repo.CreateAsync(produto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { data = ToDto(created) });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProdutoDto dto)
    {
        var tipo = await _tipoRepo.GetByIdAsync(dto.TipoProdutoId);
        if (tipo is null)
            return UnprocessableEntity(new { error = new { code = "TIPO_PRODUTO_NOT_FOUND", message = "tipoProdutoId inválido." } });

        var updated = await _repo.UpdateAsync(id, new Produto { TipoProdutoId = dto.TipoProdutoId, Nome = dto.Nome, Descricao = dto.Descricao, Ativado = dto.Ativado });
        if (updated is null) return NotFound();
        return Ok(new { data = ToDto(updated) });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static ProdutoResponseDto ToDto(Produto p) =>
        new(p.Id, new TipoProdutoDto(p.TipoProduto.Id, p.TipoProduto.Sigla, p.TipoProduto.Nome, p.TipoProduto.Descricao),
            p.Nome, p.Descricao, p.DataCriacao, p.DataAtualizacao, p.Ativado);
}
