namespace AssistenteDB.Domain.Entities;

public class Arquivo
{
    public long Id { get; set; }
    public long TipoArquivoId { get; set; }
    public byte[]? Bytes { get; set; }

    public TipoArquivo TipoArquivo { get; set; } = null!;
}
