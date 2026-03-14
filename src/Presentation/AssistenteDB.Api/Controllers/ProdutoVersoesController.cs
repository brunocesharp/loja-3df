using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
public class ProdutoVersoesController : ControllerBase
{
    private readonly IProdutoVersaoRepository _repo;
    private readonly IProdutoRepository _produtoRepo;
    private readonly IProdutoArquivoRepository _arquivoVincRepo;
    private readonly IArquivoRepository _arquivoRepo;

    public ProdutoVersoesController(IProdutoVersaoRepository repo, IProdutoRepository produtoRepo,
        IProdutoArquivoRepository arquivoVincRepo, IArquivoRepository arquivoRepo)
    {
        _repo = repo;
        _produtoRepo = produtoRepo;
        _arquivoVincRepo = arquivoVincRepo;
        _arquivoRepo = arquivoRepo;
    }

    [HttpGet("api/produtos/{produtoId:long}/versoes")]
    public async Task<IActionResult> GetByProduto(long produtoId)
    {
        var produto = await _produtoRepo.GetByIdAsync(produtoId);
        if (produto is null) return NotFound(new { error = new { code = "PRODUTO_NOT_FOUND", message = "Produto não encontrado." } });
        var versoes = await _repo.GetByProdutoIdAsync(produtoId);
        return Ok(new { data = versoes.Select(ToDto) });
    }

    [HttpGet("api/versoes/{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var versao = await _repo.GetByIdAsync(id);
        if (versao is null) return NotFound();
        return Ok(new { data = ToDto(versao) });
    }

    [HttpPost("api/produtos/{produtoId:long}/versoes")]
    public async Task<IActionResult> Create(long produtoId, [FromBody] CreateProdutoVersaoDto dto)
    {
        var produto = await _produtoRepo.GetByIdAsync(produtoId);
        if (produto is null) return NotFound(new { error = new { code = "PRODUTO_NOT_FOUND", message = "Produto não encontrado." } });

        if (await _repo.NumeroExisteAsync(produtoId, dto.Numero))
            return UnprocessableEntity(new { error = new { code = "VERSAO_NUMERO_DUPLICADO", message = $"Já existe uma versão com o número {dto.Numero} para este produto." } });

        var versao = new ProdutoVersao { ProdutoId = produtoId, Nome = dto.Nome, Numero = dto.Numero };
        var created = await _repo.CreateAsync(versao);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { data = ToDto(created) });
    }

    [HttpPut("api/versoes/{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProdutoVersaoDto dto)
    {
        var versao = await _repo.GetByIdAsync(id);
        if (versao is null) return NotFound();

        if (await _repo.NumeroExisteAsync(versao.ProdutoId, dto.Numero, excludeId: id))
            return UnprocessableEntity(new { error = new { code = "VERSAO_NUMERO_DUPLICADO", message = $"Já existe uma versão com o número {dto.Numero} para este produto." } });

        var updated = await _repo.UpdateAsync(id, new ProdutoVersao { Nome = dto.Nome, Numero = dto.Numero });
        if (updated is null) return NotFound();
        return Ok(new { data = ToDto(updated) });
    }

    [HttpDelete("api/versoes/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // --- Vínculo com Arquivo (RF-009) ---

    [HttpPut("api/versoes/{id:long}/arquivo")]
    public async Task<IActionResult> VincularArquivo(long id, [FromBody] VincularArquivoDto dto)
    {
        var versao = await _repo.GetByIdAsync(id);
        if (versao is null) return NotFound();

        var arquivo = await _arquivoRepo.GetByIdAsync(dto.ArquivoId);
        if (arquivo is null)
            return UnprocessableEntity(new { error = new { code = "ARQUIVO_NOT_FOUND", message = "arquivoId inválido." } });

        var resultado = await _arquivoVincRepo.VincularAsync(id, dto.ArquivoId);
        if (!resultado)
            return Conflict(new { error = new { code = "ARQUIVO_JA_VINCULADO", message = "Arquivo já está vinculado a outra versão." } });

        var tipoDto = new TipoArquivoDto(arquivo.TipoArquivo.Id, arquivo.TipoArquivo.Nome, arquivo.TipoArquivo.Sigla);
        return Ok(new { data = new { produtoVersaoId = id, arquivo = new ArquivoResponseDto(arquivo.Id, tipoDto) } });
    }

    [HttpDelete("api/versoes/{id:long}/arquivo")]
    public async Task<IActionResult> DesvincularArquivo(long id)
    {
        var desvinculado = await _arquivoVincRepo.DesvincularAsync(id);
        if (!desvinculado)
            return NotFound(new { error = new { code = "VINCULO_NOT_FOUND", message = "Versão não possui arquivo vinculado." } });
        return NoContent();
    }

    private static ProdutoVersaoResponseDto ToDto(ProdutoVersao v)
    {
        var produtoDto = new ProdutoResponseDto(v.Produto.Id,
            new TipoProdutoDto(v.Produto.TipoProduto.Id, v.Produto.TipoProduto.Sigla, v.Produto.TipoProduto.Nome, v.Produto.TipoProduto.Descricao),
            v.Produto.Nome, v.Produto.Descricao, v.Produto.DataCriacao, v.Produto.DataAtualizacao, v.Produto.Ativado);

        ArquivoResponseDto? arquivoDto = null;
        if (v.ProdutoArquivo?.Arquivo is not null)
        {
            var ta = v.ProdutoArquivo.Arquivo.TipoArquivo;
            arquivoDto = new ArquivoResponseDto(v.ProdutoArquivo.Arquivo.Id, new TipoArquivoDto(ta.Id, ta.Nome, ta.Sigla));
        }

        return new ProdutoVersaoResponseDto(v.Id, v.ProdutoId, produtoDto, v.Nome, v.Numero, arquivoDto);
    }
}
