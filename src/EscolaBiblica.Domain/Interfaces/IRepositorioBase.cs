namespace EscolaBiblica.Domain.Interfaces;

public interface IRepositorioBase<T> where T : class
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default);
    Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default);
    Task RemoverAsync(T entidade, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}