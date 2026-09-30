using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Menus.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class PermissionGroupRepository(AppDbContext context) : Repository<PermissionGroup>(context), IPermissionGroupRepository
{
    public Task<List<PermissionGroup>> GetAllWithPermissionsAsync() =>
        _entities.AsNoTracking().Include(g => g.Permissions).OrderBy(g => g.GroupName).ToListAsync();

    public Task<List<GroupRow>> GetRowsAsync() =>
        _entities.AsNoTracking().OrderBy(g => g.GroupName)
            .Select(g => new GroupRow(g.PermissionGroupId, g.GroupName, g.Description, g.Permissions.Count)).ToListAsync();

    public Task<PermissionGroup?> GetByIdAsync(int id) => _entities.FirstOrDefaultAsync(g => g.PermissionGroupId == id);

    public Task<bool> NameExistsAsync(string name, int exceptId) => _entities.AnyAsync(g => g.GroupName == name && g.PermissionGroupId != exceptId);

    public Task<PermissionGroup?> GetByNameAsync(string groupName) => _entities.FirstOrDefaultAsync(g => g.GroupName == groupName);
}
