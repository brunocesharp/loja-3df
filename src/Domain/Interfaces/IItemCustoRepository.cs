using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface IItemCustoRepository
{
    Task<IEnumerable<ItemCusto>> GetByItemIdAsync(long itemId);
    Task<ItemCusto?> GetByIdAsync(long id);
    Task<ItemCusto> CreateAsync(ItemCusto custo);
    Task<ItemCusto?> UpdateAsync(long id, ItemCusto custo);
    Task<bool> DeleteAsync(long id);
}
