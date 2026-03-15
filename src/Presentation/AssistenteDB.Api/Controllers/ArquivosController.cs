using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
[Route("api/arquivos")]
public class ArquivosController : ControllerBase
{
    private readonly IArquivoRepository _repo;
    private readonly ITipoArquivoRepository _tipoArquivoRepo;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public ArquivosController(IArquivoRepository repo, ITipoArquivoRepository tipoArquivoRepo)
    {
        _repo = repo;
        _tipoArquivoRepo = tipoArquivoRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] long tipoArquivoId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = new { code = "FILE_REQUIRED", message = "Arquivo é obrigatório." } });

        if (file.Length > MaxFileSize)
            return StatusCode(413, new { error = new { code = "ARQUIVO_MUITO_GRANDE", message = "Tamanho máximo permitido é 10 MB." } });

        var tipo = await _tipoArquivoRepo.GetByIdAsync(tipoArquivoId);
        if (tipo is null)
            return UnprocessableEntity(new { error = new { code = "TIPO_ARQUIVO_NOT_FOUND", message = "tipoArquivoId inválido." } });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var arquivo = new Arquivo { TipoArquivoId = tipoArquivoId, Bytes = ms.ToArray() };
        var created = await _repo.CreateAsync(arquivo);

        var tipoDto = new TipoArquivoDto(tipo.Id, tipo.Nome, tipo.Sigla);
        return CreatedAtAction(nameof(Download), new { id = created.Id }, new ArquivoResponseDto(created.Id, tipoDto));
    }

    [HttpGet("{id:long}/download")]
    public async Task<IActionResult> Download(long id)
    {
        var arquivo = await _repo.GetByIdAsync(id);
        if (arquivo is null) return NotFound();
        if (arquivo.Bytes is null || arquivo.Bytes.Length == 0)
            return NotFound(new { error = new { code = "ARQUIVO_VAZIO", message = "Arquivo sem conteúdo." } });

        return File(arquivo.Bytes, "application/octet-stream", $"arquivo_{id}");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var arquivo = await _repo.GetByIdAsync(id);
        if (arquivo is null) return NotFound();

        if (await _repo.IsVinculadoAsync(id))
            return Conflict(new { error = new { code = "ARQUIVO_VINCULADO", message = "Arquivo está vinculado a um item ou versão e não pode ser removido." } });

        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
