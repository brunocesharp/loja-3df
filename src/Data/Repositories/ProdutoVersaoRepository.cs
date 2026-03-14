using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ProdutoVersaoRepository : IProdutoVersaoRepository
{
    private readonly AppDbContext _context;

    public ProdutoVersaoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<ProdutoVersao>> GetByProdutoIdAsync(long produtoId)
        => await _context.ProdutosVersao
            .AsNoTracking()
            .Include(v => v.Produto).ThenInclude(p => p.TipoProduto)
            .Where(v => v.ProdutoId == produtoId)
            .ToListAsync();

    public async Task<ProdutoVersao?> GetByIdAsync(long id)
        => await _context.ProdutosVersao
            .AsNoTracking()
            .Include(v => v.Produto).ThenInclude(p => p.TipoProduto)
            .Include(v => v.ProdutoArquivo!).ThenInclude(pa => pa.Arquivo).ThenInclude(a => a.TipoArquivo)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<bool> NumeroExisteAsync(long produtoId, int numero, long? excludeId = null)
    {
        var query = _context.ProdutosVersao.Where(v => v.ProdutoId == produtoId && v.Numero == numero);
        if (excludeId.HasValue) query = query.Where(v => v.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<ProdutoVersao> CreateAsync(ProdutoVersao versao)
    {
        _context.ProdutosVersao.Add(versao);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(versao.Id))!;
    }

    public async Task<ProdutoVersao?> UpdateAsync(long id, ProdutoVersao updated)
    {
        var versao = await _context.ProdutosVersao.FindAsync(id);
        if (versao is null) return null;
        versao.Nome = updated.Nome;
        versao.Numero = updated.Numero;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var versao = await _context.ProdutosVersao.FindAsync(id);
        if (versao is null) return false;
        _context.ProdutosVersao.Remove(versao);
        await _context.SaveChangesAsync();
        return true;
    }
}
