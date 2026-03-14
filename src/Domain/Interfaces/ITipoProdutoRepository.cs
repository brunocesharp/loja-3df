using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface ITipoProdutoRepository
{
    Task<IEnumerable<TipoProduto>> GetAllAsync();
    Task<TipoProduto?> GetByIdAsync(long id);
}
