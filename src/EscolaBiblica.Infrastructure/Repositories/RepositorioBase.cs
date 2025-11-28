using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.Infrastructure.Repositories;

public class RepositorioBase<T> : IRepositorioBase<T> where T : class
{
    protected readonly EscolaBiblicaDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositorioBase(EscolaBiblicaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entidade, cancellationToken);
        return entidade;
    }

    public virtual Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entidade);
        return Task.CompletedTask;
    }

    public virtual Task RemoverAsync(T entidade, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entidade);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;
    }
}