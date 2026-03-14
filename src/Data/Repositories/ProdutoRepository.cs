using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context) => _context = context;

    public async Task<(IEnumerable<Produto> Items, int Total)> GetAllAsync(int page, int pageSize, bool? ativado, long? tipoProdutoId)
    {
        var query = _context.Produtos.AsNoTracking().Include(p => p.TipoProduto).AsQueryable();

        if (ativado.HasValue) query = query.Where(p => p.Ativado == ativado.Value);
        if (tipoProdutoId.HasValue) query = query.Where(p => p.TipoProdutoId == tipoProdutoId.Value);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Produto?> GetByIdAsync(long id)
        => await _context.Produtos.AsNoTracking().Include(p => p.TipoProduto).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Produto> CreateAsync(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(produto.Id))!;
    }

    public async Task<Produto?> UpdateAsync(long id, Produto updated)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null) return null;
        produto.TipoProdutoId = updated.TipoProdutoId;
        produto.Nome = updated.Nome;
        produto.Descricao = updated.Descricao;
        produto.Ativado = updated.Ativado;
        produto.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null) return false;
        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return true;
    }
}
