using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ItemArquivoRepository : IItemArquivoRepository
{
    private readonly AppDbContext _context;

    public ItemArquivoRepository(AppDbContext context) => _context = context;

    public async Task<bool> VincularAsync(long itemId, long arquivoId)
    {
        // Verifica se arquivo já está vinculado a outro item
        var jaVinculado = await _context.ItensArquivo.AnyAsync(ia => ia.ArquivoId == arquivoId && ia.ItemId != itemId);
        if (jaVinculado) return false;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // Remove vínculo anterior do item (se existir)
        var anterior = await _context.ItensArquivo.FirstOrDefaultAsync(ia => ia.ItemId == itemId);
        if (anterior is not null) _context.ItensArquivo.Remove(anterior);

        _context.ItensArquivo.Add(new ItemArquivo { ItemId = itemId, ArquivoId = arquivoId });
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }

    public async Task<bool> DesvincularAsync(long itemId)
    {
        var vinculo = await _context.ItensArquivo.FirstOrDefaultAsync(ia => ia.ItemId == itemId);
        if (vinculo is null) return false;
        _context.ItensArquivo.Remove(vinculo);
        await _context.SaveChangesAsync();
        return true;
    }
}
