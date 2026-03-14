using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
        => await _context.Items.AsNoTracking().ToListAsync();

    public async Task<Item?> GetByIdAsync(int id)
        => await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Item> CreateAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<Item?> UpdateAsync(int id, Item updated)
    {
        var item = await _context.Items.FindAsync(id);
        if (item is null) return null;

        item.Name = updated.Name;
        item.Description = updated.Description;
        item.IsActive = updated.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item is null) return false;

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
