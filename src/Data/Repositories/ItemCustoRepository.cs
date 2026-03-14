using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ItemCustoRepository : IItemCustoRepository
{
    private readonly AppDbContext _context;

    public ItemCustoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<ItemCusto>> GetByItemIdAsync(long itemId)
        => await _context.ItensCusto.AsNoTracking().Where(c => c.ItemId == itemId).ToListAsync();

    public async Task<ItemCusto?> GetByIdAsync(long id)
        => await _context.ItensCusto.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ItemCusto> CreateAsync(ItemCusto custo)
    {
        _context.ItensCusto.Add(custo);
        await _context.SaveChangesAsync();
        return custo;
    }

    public async Task<ItemCusto?> UpdateAsync(long id, ItemCusto updated)
    {
        var custo = await _context.ItensCusto.FindAsync(id);
        if (custo is null) return null;
        custo.Peso = updated.Peso;
        custo.Tempo = updated.Tempo;
        custo.Quantidade = updated.Quantidade;
        custo.Perdas = updated.Perdas;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var custo = await _context.ItensCusto.FindAsync(id);
        if (custo is null) return false;
        _context.ItensCusto.Remove(custo);
        await _context.SaveChangesAsync();
        return true;
    }
}
