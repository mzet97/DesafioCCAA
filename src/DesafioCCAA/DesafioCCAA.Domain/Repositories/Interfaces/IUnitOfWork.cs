namespace DesafioCCAA.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IRepositoryFactory RepositoryFactory { get; }

    Task BeginTransactionAsync();

    Task CommitAsync();

    Task RollbackAsync();
}
