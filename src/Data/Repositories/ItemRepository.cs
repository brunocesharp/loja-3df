using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Item>> GetByVersaoIdAsync(long versaoId)
        => await _context.Itens
            .AsNoTracking()
            .Include(i => i.ProdutoVersao).ThenInclude(v => v.Produto).ThenInclude(p => p.TipoProduto)
            .Where(i => i.ProdutoVersaoId == versaoId)
            .ToListAsync();

    public async Task<Item?> GetByIdAsync(long id)
        => await _context.Itens
            .AsNoTracking()
            .Include(i => i.ProdutoVersao).ThenInclude(v => v.Produto).ThenInclude(p => p.TipoProduto)
            .Include(i => i.ProdutoVersao).ThenInclude(v => v.ProdutoArquivo!)
            .Include(i => i.Custos)
            .Include(i => i.ItemArquivo!).ThenInclude(ia => ia.Arquivo).ThenInclude(a => a.TipoArquivo)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Item> CreateAsync(Item item)
    {
        _context.Itens.Add(item);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(item.Id))!;
    }

    public async Task<Item?> UpdateAsync(long id, Item updated)
    {
        var item = await _context.Itens.FindAsync(id);
        if (item is null) return null;
        item.Nome = updated.Nome;
        item.Descricao = updated.Descricao;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.Itens.FindAsync(id);
        if (item is null) return false;
        _context.Itens.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
