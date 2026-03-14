using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ArquivoRepository : IArquivoRepository
{
    private readonly AppDbContext _context;

    public ArquivoRepository(AppDbContext context) => _context = context;

    public async Task<Arquivo?> GetByIdAsync(long id)
        => await _context.Arquivos
            .AsNoTracking()
            .Include(a => a.TipoArquivo)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Arquivo> CreateAsync(Arquivo arquivo)
    {
        _context.Arquivos.Add(arquivo);
        await _context.SaveChangesAsync();
        return arquivo;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var arquivo = await _context.Arquivos.FindAsync(id);
        if (arquivo is null) return false;
        _context.Arquivos.Remove(arquivo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsVinculadoAsync(long id)
    {
        bool emItem = await _context.ItensArquivo.AnyAsync(ia => ia.ArquivoId == id);
        if (emItem) return true;
        return await _context.ProdutosArquivo.AnyAsync(pa => pa.ArquivoId == id);
    }
}
