namespace AssistenteDB.Domain.Interfaces;

public interface IProdutoArquivoRepository
{
    /// <summary>Vincula arquivo à versão. Substitui vínculo existente. Retorna false se arquivo já está vinculado a outra versão.</summary>
    Task<bool> VincularAsync(long versaoId, long arquivoId);
    /// <summary>Remove vínculo. Retorna false se não existe vínculo.</summary>
    Task<bool> DesvincularAsync(long versaoId);
}
