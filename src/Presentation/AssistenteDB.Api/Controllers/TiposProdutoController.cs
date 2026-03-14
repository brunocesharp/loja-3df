using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
[Route("api/tipos-produto")]
public class TiposProdutoController : ControllerBase
{
    private readonly ITipoProdutoRepository _repo;

    public TiposProdutoController(ITipoProdutoRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tipos = await _repo.GetAllAsync();
        return Ok(tipos.Select(t => new TipoProdutoDto(t.Id, t.Sigla, t.Nome, t.Descricao)));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var tipo = await _repo.GetByIdAsync(id);
        if (tipo is null) return NotFound();
        return Ok(new TipoProdutoDto(tipo.Id, tipo.Sigla, tipo.Nome, tipo.Descricao));
    }
}
