using CMS.Data.Context;
using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class PermissionRepository(AppDbContext context) : Repository<Permission>(context), IPermissionRepository
{
    public Task<List<string>> GetAllValuesAsync() => _entities.AsNoTracking().Select(p => p.Value).ToListAsync();
}
