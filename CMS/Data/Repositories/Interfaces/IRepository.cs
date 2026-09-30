using System.Linq.Expressions;

namespace CMS.Data.Repositories.Interfaces;

// Ported from SafetyNet (Data/Repositories/Interfaces/IRepository.cs): same members, same names.
public interface IRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    int Count();
    IQueryable<TEntity> GetQuery();
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    TEntity? GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);
    TEntity? GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);
    TEntity? Get(int id);
    Task<TEntity?> GetAsync(int id);
    IEnumerable<TEntity> GetAll();
    IEnumerable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties);
}
