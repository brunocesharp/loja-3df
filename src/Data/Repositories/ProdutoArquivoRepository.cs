using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ProdutoArquivoRepository : IProdutoArquivoRepository
{
    private readonly AppDbContext _context;

    public ProdutoArquivoRepository(AppDbContext context) => _context = context;

    public async Task<bool> VincularAsync(long versaoId, long arquivoId)
    {
        var jaVinculado = await _context.ProdutosArquivo.AnyAsync(pa => pa.ArquivoId == arquivoId && pa.ProdutoVersaoId != versaoId);
        if (jaVinculado) return false;

        await using var tx = await _context.Database.BeginTransactionAsync();

        var anterior = await _context.ProdutosArquivo.FirstOrDefaultAsync(pa => pa.ProdutoVersaoId == versaoId);
        if (anterior is not null) _context.ProdutosArquivo.Remove(anterior);

        _context.ProdutosArquivo.Add(new ProdutoArquivo { ProdutoVersaoId = versaoId, ArquivoId = arquivoId });
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }

    public async Task<bool> DesvincularAsync(long versaoId)
    {
        var vinculo = await _context.ProdutosArquivo.FirstOrDefaultAsync(pa => pa.ProdutoVersaoId == versaoId);
        if (vinculo is null) return false;
        _context.ProdutosArquivo.Remove(vinculo);
        await _context.SaveChangesAsync();
        return true;
    }
}
