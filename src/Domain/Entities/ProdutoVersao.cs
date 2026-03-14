namespace AssistenteDB.Domain.Entities;

public class ProdutoVersao
{
    public long Id { get; set; }
    public long ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Numero { get; set; }

    public Produto Produto { get; set; } = null!;
    public ProdutoArquivo? ProdutoArquivo { get; set; }
}
