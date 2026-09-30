using CMS.Data.Context;
using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class ProviderRepository(AppDbContext context) : Repository<Provider>(context), IProviderRepository
{
    public Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null)
    {
        var q = _entities.AsNoTracking();
        if (onlyIds is not null) q = q.Where(p => onlyIds.Contains(p.ProviderId));
        return q.OrderBy(p => p.Name).Select(p => new IdName(p.ProviderId, p.Name, p.Abbreviation)).ToListAsync();
    }
}
