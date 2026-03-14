namespace AssistenteDB.Domain.Entities;

public class ProdutoArquivo
{
    public long Id { get; set; }
    public long ProdutoVersaoId { get; set; }
    public long ArquivoId { get; set; }

    public ProdutoVersao ProdutoVersao { get; set; } = null!;
    public Arquivo Arquivo { get; set; } = null!;
}
