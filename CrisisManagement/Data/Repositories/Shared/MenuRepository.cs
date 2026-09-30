using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class MenuRepository(AppDbContext context) : Repository<MenuItem>(context), IMenuRepository
{
    public Task<List<MenuItem>> GetEnabledWithPermissionsAsync() =>
        _entities.AsNoTracking().Include(m => m.Permissions).Where(m => m.IsEnabled).ToListAsync();

    public Task<List<MenuItem>> GetAllAsync() => _entities.AsNoTracking().ToListAsync();

    public Task<MenuItem?> GetByNameAsync(string menuItemName) => _entities.AsNoTracking().FirstOrDefaultAsync(m => m.MenuItemName == menuItemName);
}
