using System.ComponentModel.DataAnnotations;

namespace AssistenteDB.Application.DTOs;

public record CreateProdutoVersaoDto(
    [Required][MaxLength(150)] string Nome,
    [Required][Range(1, int.MaxValue)] int Numero
);

public record UpdateProdutoVersaoDto(
    [Required][MaxLength(150)] string Nome,
    [Required][Range(1, int.MaxValue)] int Numero
);

public record ProdutoVersaoResponseDto(
    long Id,
    long ProdutoId,
    ProdutoResponseDto Produto,
    string Nome,
    int Numero,
    ArquivoResponseDto? Arquivo
);
