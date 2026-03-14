using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface IArquivoRepository
{
    Task<Arquivo?> GetByIdAsync(long id);
    Task<Arquivo> CreateAsync(Arquivo arquivo);
    Task<bool> DeleteAsync(long id);
    Task<bool> IsVinculadoAsync(long id);
}
