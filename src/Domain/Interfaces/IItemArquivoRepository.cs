namespace AssistenteDB.Domain.Interfaces;

public interface IItemArquivoRepository
{
    /// <summary>Vincula arquivo ao item. Substitui vínculo existente. Retorna false se arquivo já está vinculado a outro item.</summary>
    Task<bool> VincularAsync(long itemId, long arquivoId);
    /// <summary>Remove vínculo. Retorna false se não existe vínculo.</summary>
    Task<bool> DesvincularAsync(long itemId);
}
