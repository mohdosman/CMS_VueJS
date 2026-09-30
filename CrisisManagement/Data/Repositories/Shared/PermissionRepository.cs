using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class PermissionRepository(AppDbContext context) : Repository<Permission>(context), IPermissionRepository
{
    public Task<List<string>> GetAllValuesAsync() => _entities.AsNoTracking().Select(p => p.Value).ToListAsync();

    public Task<List<Permission>> GetForMenuAsync(int menuId) =>
        _entities.AsNoTracking().Include(p => p.PermissionGroupName).Where(p => p.MenuItemId == menuId)
            .OrderBy(p => p.PermissionGroupName.GroupName).ThenBy(p => p.Value).ToListAsync();

    public Task<Permission?> GetByIdAsync(int id) => _entities.FirstOrDefaultAsync(p => p.PermissionId == id);

    public Task<bool> ValueExistsAsync(string value, int exceptId) => _entities.AnyAsync(p => p.Value == value && p.PermissionId != exceptId);

    public Task<Permission?> GetByValueAsync(string value) => _entities.FirstOrDefaultAsync(p => p.Value == value);
}
