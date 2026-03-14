using System.ComponentModel.DataAnnotations;

namespace AssistenteDB.Application.DTOs;

public record CreateProdutoDto(
    [Required] long TipoProdutoId,
    [Required][MaxLength(150)] string Nome,
    string? Descricao,
    bool Ativado = true
);

public record UpdateProdutoDto(
    [Required] long TipoProdutoId,
    [Required][MaxLength(150)] string Nome,
    string? Descricao,
    bool Ativado
);

public record ProdutoResponseDto(
    long Id,
    TipoProdutoDto TipoProduto,
    string Nome,
    string? Descricao,
    DateTime DataCriacao,
    DateTime? DataAtualizacao,
    bool Ativado
);
