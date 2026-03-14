namespace AssistenteDB.Domain.Entities;

public class ItemArquivo
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public long ArquivoId { get; set; }

    public Item Item { get; set; } = null!;
    public Arquivo Arquivo { get; set; } = null!;
}
