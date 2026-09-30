using System.Linq.Expressions;

namespace CMS.Data.Repositories;

// Same shape as the SafetyNet IRepository, trimmed to what the ported features use so far.
public interface IRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    IQueryable<TEntity> GetQuery();
    Task<TEntity?> GetAsync(int id, CancellationToken ct = default);
    Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}
