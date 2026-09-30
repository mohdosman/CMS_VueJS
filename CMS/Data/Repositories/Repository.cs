using System.Linq.Expressions;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

// Ported from SafetyNet (Data/Repositories/Repository.cs).
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _entities;

    public Repository(DbContext context)
    {
        _context = context;
        _entities = context.Set<TEntity>();
    }

    public virtual void Add(TEntity entity) => _entities.Add(entity);
    public virtual void AddRange(IEnumerable<TEntity> entities) => _entities.AddRange(entities);

    public virtual void Update(TEntity entity) => _entities.Update(entity);
    public virtual void UpdateRange(IEnumerable<TEntity> entities) => _entities.UpdateRange(entities);

    public virtual void Remove(TEntity entity) => _entities.Remove(entity);
    public virtual void RemoveRange(IEnumerable<TEntity> entities) => _entities.RemoveRange(entities);

    public virtual int Count() => _entities.Count();

    public IQueryable<TEntity> GetQuery() => _entities;

    public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) => _entities.Where(predicate);

    public virtual TEntity? GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate) =>
        _entities.SingleOrDefault(predicate);

    public virtual async Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _entities.SingleOrDefaultAsync(predicate);

    public virtual TEntity? GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties) =>
        Include(includeProperties).Where(predicate).SingleOrDefault();

    public virtual async Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties) =>
        await Include(includeProperties).Where(predicate).SingleOrDefaultAsync();

    public virtual IEnumerable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties) =>
        Include(includeProperties).AsEnumerable();

    public virtual TEntity? Get(int id) => _entities.Find(id);

    public virtual async Task<TEntity?> GetAsync(int id) => await _entities.FindAsync(id);

    public virtual IEnumerable<TEntity> GetAll() => _entities.ToList();

    private IQueryable<TEntity> Include(Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _entities;
        foreach (var include in includeProperties) query = query.Include(include);
        return query;
    }
}
