namespace DesafioCCAA.Domain.Repositories.Interfaces;

public interface IUnitOfWork
{
    IRepositoryFactory RepositoryFactory { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
