namespace AssistenteDB.Domain.Entities;

public class Item
{
    public long Id { get; set; }
    public long ProdutoVersaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public ProdutoVersao ProdutoVersao { get; set; } = null!;
    public ICollection<ItemCusto> Custos { get; set; } = [];
    public ItemArquivo? ItemArquivo { get; set; }
}
