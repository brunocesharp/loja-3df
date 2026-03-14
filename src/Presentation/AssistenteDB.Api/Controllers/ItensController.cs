using Microsoft.AspNetCore.Mvc;
using AssistenteDB.Application.DTOs;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Api.Controllers;

[ApiController]
public class ItensController : ControllerBase
{
    private readonly IItemRepository _repo;
    private readonly IProdutoVersaoRepository _versaoRepo;
    private readonly IItemArquivoRepository _itemArquivoRepo;
    private readonly IArquivoRepository _arquivoRepo;

    public ItensController(IItemRepository repo, IProdutoVersaoRepository versaoRepo,
        IItemArquivoRepository itemArquivoRepo, IArquivoRepository arquivoRepo)
    {
        _repo = repo;
        _versaoRepo = versaoRepo;
        _itemArquivoRepo = itemArquivoRepo;
        _arquivoRepo = arquivoRepo;
    }

    [HttpGet("api/versoes/{versaoId:long}/itens")]
    public async Task<IActionResult> GetByVersao(long versaoId)
    {
        var versao = await _versaoRepo.GetByIdAsync(versaoId);
        if (versao is null) return NotFound(new { error = new { code = "VERSAO_NOT_FOUND", message = "Versão não encontrada." } });
        var itens = await _repo.GetByVersaoIdAsync(versaoId);
        return Ok(new { data = itens.Select(i => new { i.Id, i.ProdutoVersaoId, i.Nome, i.Descricao }) });
    }

    [HttpGet("api/itens/{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null) return NotFound();
        return Ok(new { data = ToDto(item) });
    }

    [HttpPost("api/versoes/{versaoId:long}/itens")]
    public async Task<IActionResult> Create(long versaoId, [FromBody] CreateItemDto dto)
    {
        var versao = await _versaoRepo.GetByIdAsync(versaoId);
        if (versao is null) return NotFound(new { error = new { code = "VERSAO_NOT_FOUND", message = "Versão não encontrada." } });

        var item = new Item { ProdutoVersaoId = versaoId, Nome = dto.Nome, Descricao = dto.Descricao };
        var created = await _repo.CreateAsync(item);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { data = ToDto(created) });
    }

    [HttpPut("api/itens/{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateItemDto dto)
    {
        var updated = await _repo.UpdateAsync(id, new Item { Nome = dto.Nome, Descricao = dto.Descricao });
        if (updated is null) return NotFound();
        return Ok(new { data = ToDto(updated) });
    }

    [HttpDelete("api/itens/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // --- Vínculo com Arquivo (RF-008) ---

    [HttpPut("api/itens/{id:long}/arquivo")]
    public async Task<IActionResult> VincularArquivo(long id, [FromBody] VincularArquivoDto dto)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null) return NotFound();

        var arquivo = await _arquivoRepo.GetByIdAsync(dto.ArquivoId);
        if (arquivo is null)
            return UnprocessableEntity(new { error = new { code = "ARQUIVO_NOT_FOUND", message = "arquivoId inválido." } });

        var resultado = await _itemArquivoRepo.VincularAsync(id, dto.ArquivoId);
        if (!resultado)
            return Conflict(new { error = new { code = "ARQUIVO_JA_VINCULADO", message = "Arquivo já está vinculado a outro item." } });

        var tipoDto = new TipoArquivoDto(arquivo.TipoArquivo.Id, arquivo.TipoArquivo.Nome, arquivo.TipoArquivo.Sigla);
        return Ok(new { data = new { itemId = id, arquivo = new ArquivoResponseDto(arquivo.Id, tipoDto) } });
    }

    [HttpDelete("api/itens/{id:long}/arquivo")]
    public async Task<IActionResult> DesvincularArquivo(long id)
    {
        var desvinculado = await _itemArquivoRepo.DesvincularAsync(id);
        if (!desvinculado)
            return NotFound(new { error = new { code = "VINCULO_NOT_FOUND", message = "Item não possui arquivo vinculado." } });
        return NoContent();
    }

    private static ItemResponseDto ToDto(Item i)
    {
        var v = i.ProdutoVersao;
        var produtoDto = new ProdutoResponseDto(v.Produto.Id,
            new TipoProdutoDto(v.Produto.TipoProduto.Id, v.Produto.TipoProduto.Sigla, v.Produto.TipoProduto.Nome, v.Produto.TipoProduto.Descricao),
            v.Produto.Nome, v.Produto.Descricao, v.Produto.DataCriacao, v.Produto.DataAtualizacao, v.Produto.Ativado);

        var versaoDto = new ProdutoVersaoResponseDto(v.Id, v.ProdutoId, produtoDto, v.Nome, v.Numero, null);

        var custos = i.Custos.Select(c => new ItemCustoResponseDto(c.Id, c.ItemId, c.Peso, c.Tempo, c.Quantidade, c.Perdas));

        ArquivoResponseDto? arquivoDto = null;
        if (i.ItemArquivo?.Arquivo is not null)
        {
            var ta = i.ItemArquivo.Arquivo.TipoArquivo;
            arquivoDto = new ArquivoResponseDto(i.ItemArquivo.Arquivo.Id, new TipoArquivoDto(ta.Id, ta.Nome, ta.Sigla));
        }

        return new ItemResponseDto(i.Id, i.ProdutoVersaoId, versaoDto, i.Nome, i.Descricao, custos, arquivoDto);
    }
}
