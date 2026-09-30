using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public class Repository<TEntity>(DbContext context) : IRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<TEntity> _entities = context.Set<TEntity>();

    public virtual void Add(TEntity entity) => _entities.Add(entity);
    public virtual void AddRange(IEnumerable<TEntity> entities) => _entities.AddRange(entities);
    public virtual void Update(TEntity entity) => _entities.Update(entity);
    public virtual void Remove(TEntity entity) => _entities.Remove(entity);
    public virtual void RemoveRange(IEnumerable<TEntity> entities) => _entities.RemoveRange(entities);

    public IQueryable<TEntity> GetQuery() => _entities;

    public virtual async Task<TEntity?> GetAsync(int id, CancellationToken ct = default) =>
        await _entities.FindAsync([id], ct);

    public virtual async Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        await _entities.SingleOrDefaultAsync(predicate, ct);
}
