using System.ComponentModel.DataAnnotations;

namespace AssistenteDB.Application.DTOs;

public record CreateItemDto(
    [Required][MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description
);

public record UpdateItemDto(
    [Required][MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    bool IsActive
);
