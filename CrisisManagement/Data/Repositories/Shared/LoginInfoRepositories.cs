using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Notifications.ViewModels;
using CrisisManagement.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class NotificationRepository(AppDbContext context) : Repository<AppNotification>(context), INotificationRepository
{
    public Task<List<AppNotification>> GetLatestAsync(int take) =>
        _entities.AsNoTracking().OrderByDescending(n => n.CreatedOn).Take(take).ToListAsync();

    public async Task<PagedResult<NotificationListItem>> SearchAsync(NotificationSearchRequest req)
    {
        var q = _entities.AsNoTracking();
        var desc = req.SortDesc;
        q = (req.SortBy ?? "").ToLowerInvariant() switch
        {
            "id" => desc ? q.OrderByDescending(n => n.AppNotificationId) : q.OrderBy(n => n.AppNotificationId),
            "notification" => desc ? q.OrderByDescending(n => n.Notification) : q.OrderBy(n => n.Notification),
            _ => req.SortBy is null || desc ? q.OrderByDescending(n => n.CreatedOn) : q.OrderBy(n => n.CreatedOn)
        };

        var size = Math.Clamp(req.PageSize, 1, 200);
        var page = Math.Max(1, req.PageIndex);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(n => new NotificationListItem(n.AppNotificationId, n.Notification, n.CreatedOn)).ToListAsync();
        return new PagedResult<NotificationListItem> { Items = items, TotalCount = total };
    }

    public Task<AppNotification?> GetByIdAsync(int id) => _entities.FirstOrDefaultAsync(n => n.AppNotificationId == id);

    public Task<int> DeleteByIdAsync(int id) => _entities.Where(n => n.AppNotificationId == id).ExecuteDeleteAsync();
}

public sealed class SupportRepository(AppDbContext context) : Repository<AppSupport>(context), ISupportRepository
{
    public Task<List<AppSupport>> GetWithContactsAsync(int take) =>
        _entities.AsNoTracking().Include(s => s.Contact).OrderBy(s => s.AppSupportId).Take(take).ToListAsync();
}
