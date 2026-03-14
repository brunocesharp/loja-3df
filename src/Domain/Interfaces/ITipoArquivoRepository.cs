using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Domain.Interfaces;

public interface ITipoArquivoRepository
{
    Task<IEnumerable<TipoArquivo>> GetAllAsync();
    Task<TipoArquivo?> GetByIdAsync(long id);
}
