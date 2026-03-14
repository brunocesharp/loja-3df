using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface IProdutoVersaoRepository
{
    Task<IEnumerable<ProdutoVersao>> GetByProdutoIdAsync(long produtoId);
    Task<ProdutoVersao?> GetByIdAsync(long id);
    Task<bool> NumeroExisteAsync(long produtoId, int numero, long? excludeId = null);
    Task<ProdutoVersao> CreateAsync(ProdutoVersao versao);
    Task<ProdutoVersao?> UpdateAsync(long id, ProdutoVersao versao);
    Task<bool> DeleteAsync(long id);
}
