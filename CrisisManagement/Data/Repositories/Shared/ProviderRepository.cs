using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class ProviderRepository(AppDbContext context) : Repository<Provider>(context), IProviderRepository
{
    public Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null)
    {
        var q = _entities.AsNoTracking();
        if (onlyIds is not null) q = q.Where(p => onlyIds.Contains(p.ProviderId));
        return q.OrderBy(p => p.Name).Select(p => new IdName(p.ProviderId, p.Name, p.Abbreviation)).ToListAsync();
    }
}
