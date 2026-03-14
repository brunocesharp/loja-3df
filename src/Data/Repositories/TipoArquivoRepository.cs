using Microsoft.EntityFrameworkCore;
using AssistenteDB.Data.Context;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;

namespace AssistenteDB.Data.Repositories;

public class TipoArquivoRepository : ITipoArquivoRepository
{
    private readonly AppDbContext _context;

    public TipoArquivoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TipoArquivo>> GetAllAsync()
        => await _context.TiposArquivo.AsNoTracking().ToListAsync();

    public async Task<TipoArquivo?> GetByIdAsync(long id)
        => await _context.TiposArquivo.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
}
