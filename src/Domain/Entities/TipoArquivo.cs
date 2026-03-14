namespace AssistenteDB.Domain.Entities;

public class TipoArquivo
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
}
