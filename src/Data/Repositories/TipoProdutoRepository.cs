using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class TipoProdutoRepository : ITipoProdutoRepository
{
    private readonly AppDbContext _context;

    public TipoProdutoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TipoProduto>> GetAllAsync()
        => await _context.TiposProduto.AsNoTracking().ToListAsync();

    public async Task<TipoProduto?> GetByIdAsync(long id)
        => await _context.TiposProduto.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
}
