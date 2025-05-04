using System.Linq.Expressions;
using DesafioCCAA.Shared.Models;
using DesafioCCAA.Shared.Responses;

namespace DesafioCCAA.Domain.Repositories.Interfaces;

public interface IRepository<TEntity> : IDisposable where TEntity : IEntity
{
    Task AddAsync(TEntity entity);
    Task<TEntity?> GetByIdNoTrackingAsync(Guid id);
    Task<TEntity?> GetByIdAsync(Guid id);

    Task<IEnumerable<TEntity>> GetAllAsync();

    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    Task UpdateAsync(TEntity entity);

    void RemoveAsync(TEntity entity);
    Task RemoveByIdAsync(Guid id);
    Task DisableAsync(Guid id);
    Task ActiveAsync(Guid id);
    Task ActiveOrDisableAsync(Guid id, bool active);

    Task<BaseResultList<TEntity>> SearchAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        int pageSize = 10, int page = 1);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
}

