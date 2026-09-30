using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class PermissionGroupRepository(AppDbContext context) : Repository<PermissionGroup>(context), IPermissionGroupRepository
{
    public Task<List<PermissionGroup>> GetAllWithPermissionsAsync() =>
        _entities.AsNoTracking().Include(g => g.Permissions).OrderBy(g => g.GroupName).ToListAsync();

    public Task<PermissionGroup?> GetByNameAsync(string groupName) => _entities.FirstOrDefaultAsync(g => g.GroupName == groupName);
}
