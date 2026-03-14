using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface IProdutoRepository
{
    Task<(IEnumerable<Produto> Items, int Total)> GetAllAsync(int page, int pageSize, bool? ativado, long? tipoProdutoId);
    Task<Produto?> GetByIdAsync(long id);
    Task<Produto> CreateAsync(Produto produto);
    Task<Produto?> UpdateAsync(long id, Produto produto);
    Task<bool> DeleteAsync(long id);
}
