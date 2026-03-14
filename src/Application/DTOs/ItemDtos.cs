using System.ComponentModel.DataAnnotations;

namespace AssistenteDB.Application.DTOs;

public record CreateItemDto(
    [Required][MaxLength(150)] string Nome,
    string? Descricao
);

public record UpdateItemDto(
    [Required][MaxLength(150)] string Nome,
    string? Descricao
);

public record ItemResponseDto(
    long Id,
    long ProdutoVersaoId,
    ProdutoVersaoResponseDto ProdutoVersao,
    string Nome,
    string? Descricao,
    IEnumerable<ItemCustoResponseDto> Custos,
    ArquivoResponseDto? Arquivo
);
