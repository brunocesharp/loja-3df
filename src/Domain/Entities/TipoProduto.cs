namespace AssistenteDB.Domain.Entities;

public class TipoProduto
{
    public long Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
