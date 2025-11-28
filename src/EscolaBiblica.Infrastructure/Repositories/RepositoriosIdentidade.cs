using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.Infrastructure.Repositories;

public class RepositorioIgreja : RepositorioBase<Igreja>, IRepositorioIgreja
{
    public RepositorioIgreja(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<Igreja?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Nome == nome, cancellationToken);
    }

    public async Task<IEnumerable<Igreja>> ObterAtivasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Ativa)
            .OrderBy(i => i.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NomeJaExisteAsync(string nome, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(i => i.Nome == nome);
        
        if (ignorarId.HasValue)
            query = query.Where(i => i.Id != ignorarId.Value);
            
        return await query.AnyAsync(cancellationToken);
    }
}

public class RepositorioUsuario : RepositorioBase<Usuario>, IRepositorioUsuario
{
    public RepositorioUsuario(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Igreja)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Usuario?> ObterPorIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Igreja)
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityId, cancellationToken);
    }

    public async Task<IEnumerable<Usuario>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IgrejaId == igrejaId)
            .Include(u => u.Igreja)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Usuario>> ObterProfessoresAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IgrejaId == igrejaId && u.PodeGerenciarTurmas() && u.Ativo)
            .Include(u => u.Igreja)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.Email == email);
        
        if (ignorarId.HasValue)
            query = query.Where(u => u.Id != ignorarId.Value);
            
        return await query.AnyAsync(cancellationToken);
    }
}