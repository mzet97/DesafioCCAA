﻿using SistemaLivro.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SistemaLivro.Infrastructure.Persistence;
using SistemaLivro.Shared.Models;
using SistemaLivro.Shared.Responses;
using System.Linq.Expressions;

namespace SistemaLivro.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity<TEntity>
{
    protected readonly ApplicationDbContext Db;
    protected readonly DbSet<TEntity> DbSet;

    protected Repository(ApplicationDbContext db)
    {
        Db = db ?? throw new ArgumentNullException(nameof(db));
        DbSet = db.Set<TEntity>();
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        await DbSet.AddAsync(entity);
    }

    public virtual async Task<BaseResultList<TEntity>> SearchAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        int pageSize = 10, int page = 1)
    {
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));

        var query = DbSet.AsNoTracking().AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();
        var paged = PagedResult.Create(page, pageSize, totalCount);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        foreach (var includeProperty in includeProperties.Split
               (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        var data = await query.Skip(paged.Skip()).Take(pageSize).ToListAsync();
        return new BaseResultList<TEntity>(data, paged);
    }


    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return await DbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID não pode ser vazio", nameof(id));

        return await DbSet.FindAsync(id);
    }

    public virtual async Task<TEntity?> GetByIdNoTrackingAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID não pode ser vazio", nameof(id));

        return await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Update();
        DbSet.Update(entity);

        return Task.CompletedTask;
    }

    public virtual void RemoveAsync(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        //if (Db.Entry(entity).State == EntityState.Detached)
        //{
        //    DbSet.Attach(entity);
        //}

        DbSet.Remove(entity);
    }

    public virtual async Task RemoveByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID não pode ser vazio", nameof(id));

        var entity = await DbSet.FindAsync(id);

        if (entity is not null)
        {
            DbSet.Remove(entity);
        }
        else
        {
            throw new InvalidOperationException("Entidade não encontrada para exclusão.");
        }
    }

    public virtual async Task DisableAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID não pode ser vazio", nameof(id));

        var entity = await DbSet.FindAsync(id);

        if (entity != null)
        {
            entity.Disabled();
            DbSet.Update(entity);
        }
    }

    public async Task ActiveAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID não pode ser vazio", nameof(id));

        var entity = await DbSet.FindAsync(id);

        if (entity != null)
        {
            entity.Activate();
            DbSet.Update(entity);
        }
    }

    public Task ActiveOrDisableAsync(Guid id, bool active)
    {
        if (active)
        {
            return ActiveAsync(id);
        }
        else
        {
            return DisableAsync(id);
        }
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null ? await DbSet.CountAsync() : await DbSet.CountAsync(predicate);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        return await DbSet.AsNoTracking().AnyAsync(predicate);
    }

    public virtual IQueryable<TEntity> GetQueryable()
    {
        return DbSet.AsNoTracking();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Db?.Dispose();
        }
    }
}