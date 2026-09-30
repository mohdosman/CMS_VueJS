using CMS.Data.Context;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class RoleRepository(AppDbContext context) : Repository<ApplicationRole>(context), IRoleRepository
{
    public Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null)
    {
        var q = _entities.AsNoTracking();
        if (onlyIds is not null) q = q.Where(r => onlyIds.Contains(r.Id));
        return q.OrderBy(r => r.Name).Select(r => new IdName(r.Id, r.Name!)).ToListAsync();
    }
}
