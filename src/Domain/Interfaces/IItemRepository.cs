using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface IItemRepository
{
    Task<IEnumerable<Item>> GetByVersaoIdAsync(long versaoId);
    Task<Item?> GetByIdAsync(long id);
    Task<Item> CreateAsync(Item item);
    Task<Item?> UpdateAsync(long id, Item item);
    Task<bool> DeleteAsync(long id);
}
