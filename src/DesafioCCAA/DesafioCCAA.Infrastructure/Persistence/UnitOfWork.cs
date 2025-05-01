using DesafioCCAA.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public IRepositoryFactory RepositoryFactory { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IRepositoryFactory repositoryFactory,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        RepositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public async Task BeginTransactionAsync()
    {
        if (_context.Database.CurrentTransaction != null)
            throw new InvalidOperationException("Já existe uma transação em andamento.");

        await _context.Database.BeginTransactionAsync();

        _logger.LogInformation("Transação iniciada.");
    }

    public async Task CommitAsync()
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Transação não iniciada.");

        try
        {
            await _context.SaveChangesAsync();
            await _context.Database.CommitTransactionAsync();
            _logger.LogInformation("Transação commited com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no commit, fazendo rollback.");
            await _context.Database.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Não há transação para rollback.");

        await _context.Database.RollbackTransactionAsync();

        _logger.LogWarning("Transação revertida (rollback).");
    }

    public void Dispose()
    {
        RepositoryFactory.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_context.Database.CurrentTransaction != null)
            await _context.Database.RollbackTransactionAsync();

        RepositoryFactory.Dispose();

        await _context.DisposeAsync();
    }
}
