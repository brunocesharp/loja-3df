namespace AssistenteDB.Domain.Entities;

public class Produto
{
    public long Id { get; set; }
    public long TipoProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativado { get; set; } = true;

    public TipoProduto TipoProduto { get; set; } = null!;
}
