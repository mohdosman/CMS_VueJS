using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Notifications.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Data.Repositories.Interfaces;

// What the sign-in page shows before anyone is signed in.
public interface INotificationRepository : IRepository<AppNotification>
{
    // Newest first.
    Task<List<AppNotification>> GetLatestAsync(int take);

    // Notifications screen: one page, sortBy is "id", "notification" or "createdon" (default).
    Task<PagedResult<NotificationListItem>> SearchAsync(NotificationSearchRequest request);
    Task<AppNotification?> GetByIdAsync(int id);
    // Direct delete; returns rows removed.
    Task<int> DeleteByIdAsync(int id);
}

public interface ISupportRepository : IRepository<AppSupport>
{
    // The designated support people with their contact details, in id order.
    Task<List<AppSupport>> GetWithContactsAsync(int take);
}
