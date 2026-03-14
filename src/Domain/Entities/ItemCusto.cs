namespace AssistenteDB.Domain.Entities;

public class ItemCusto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public decimal? Peso { get; set; }
    public decimal? Tempo { get; set; }
    public decimal? Quantidade { get; set; }
    public decimal? Perdas { get; set; }

    public Item Item { get; set; } = null!;
}
