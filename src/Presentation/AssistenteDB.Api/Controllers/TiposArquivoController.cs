using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
[Route("api/tipos-arquivo")]
public class TiposArquivoController : ControllerBase
{
    private readonly ITipoArquivoRepository _repo;

    public TiposArquivoController(ITipoArquivoRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tipos = await _repo.GetAllAsync();
        return Ok(tipos.Select(t => new TipoArquivoDto(t.Id, t.Nome, t.Sigla)));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var tipo = await _repo.GetByIdAsync(id);
        if (tipo is null) return NotFound();
        return Ok(new TipoArquivoDto(tipo.Id, tipo.Nome, tipo.Sigla));
    }
}
