using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Context;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class ProviderUserRepository(AppDbContext context) : Repository<ProviderUser>(context), IProviderUserRepository
{
    private readonly AppDbContext _db = context;

    public Task<List<int>> GetProviderIdsForUserAsync(int userId) =>
        _entities.AsNoTracking().Where(pu => pu.UserId == userId).Select(pu => pu.ProviderId).Distinct().ToListAsync();

    public Task<List<IdName>> GetProvidersForUserAsync(int userId) =>
        (from pu in _entities.AsNoTracking()
         join p in _db.Providers on pu.ProviderId equals p.ProviderId
         where pu.UserId == userId
         orderby p.Name
         select new IdName(p.ProviderId, p.Name)).ToListAsync();

    public Task<List<ProviderUser>> GetForUserAsync(int userId) =>
        _entities.Where(x => x.UserId == userId).ToListAsync();
}
