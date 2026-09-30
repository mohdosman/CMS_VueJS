using CMS.Data.Context;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class PermissionGroupRepository(AppDbContext context) : Repository<PermissionGroup>(context), IPermissionGroupRepository
{
    public Task<List<PermissionGroup>> GetAllWithPermissionsAsync() =>
        _entities.AsNoTracking().Include(g => g.Permissions).OrderBy(g => g.GroupName).ToListAsync();

    public Task<PermissionGroup?> GetByNameAsync(string groupName) => _entities.FirstOrDefaultAsync(g => g.GroupName == groupName);
}
