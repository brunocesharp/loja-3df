using System.ComponentModel.DataAnnotations;

namespace AssistenteDB.Application.DTOs;

public record CreateItemCustoDto(
    [Range(0, double.MaxValue)] decimal? Peso,
    [Range(0, double.MaxValue)] decimal? Tempo,
    [Range(0, double.MaxValue)] decimal? Quantidade,
    [Range(0, double.MaxValue)] decimal? Perdas
);

public record UpdateItemCustoDto(
    [Range(0, double.MaxValue)] decimal? Peso,
    [Range(0, double.MaxValue)] decimal? Tempo,
    [Range(0, double.MaxValue)] decimal? Quantidade,
    [Range(0, double.MaxValue)] decimal? Perdas
);

public record ItemCustoResponseDto(
    long Id,
    long ItemId,
    decimal? Peso,
    decimal? Tempo,
    decimal? Quantidade,
    decimal? Perdas
);
