﻿using SistemaLivro.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace SistemaLivro.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;

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

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogInformation("Transação iniciada.");
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Transação não iniciada.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            await _transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transação comitada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no commit, realizando rollback.");
            await _transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();

            _context.ChangeTracker.Clear();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Transação revertida (rollback).");
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
