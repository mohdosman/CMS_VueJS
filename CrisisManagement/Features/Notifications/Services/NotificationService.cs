using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Notifications.ViewModels;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;

namespace CrisisManagement.Features.Notifications.Services;

// Port of the Blazor CMS NotificationsService. The newest notification is shown as "Attention" on the sign-in page.
public sealed class NotificationService(IUnitOfWork uow)
{
    public Task<PagedResult<NotificationListItem>> SearchAsync(NotificationSearchRequest req) => uow.Notifications.SearchAsync(req);

    public async Task<NotificationDetail?> GetAsync(int id) =>
        await uow.Notifications.GetByIdAsync(id) is { } n ? ToDetail(n) : null;

    public async Task<NotificationDetail> CreateAsync(NotificationEditRequest r)
    {
        var text = Validate(r);
        var n = new AppNotification { Notification = text };
        uow.Notifications.Add(n);
        await uow.SaveChangesAsync();
        return ToDetail(n);
    }

    // Null = notification not found.
    public async Task<NotificationDetail?> UpdateAsync(int id, NotificationEditRequest r)
    {
        var n = await uow.Notifications.GetByIdAsync(id);
        if (n is null) return null;

        var text = Validate(r);

        // Compared here rather than via the tracked OriginalValue, which reloads overwrite.
        if (!string.IsNullOrWhiteSpace(r.RowVersion) && !n.Version.AsSpan().SequenceEqual(Convert.FromBase64String(r.RowVersion)))
            throw new ConflictException("This record was updated by someone else while you were editing it.  Your changes were not saved.  Click the Cancel button and enter this screen again to see the changes.");

        n.Notification = text;
        await uow.SaveChangesAsync();
        return ToDetail(n);
    }

    // False = notification not found.
    public async Task<bool> DeleteAsync(int id) => await uow.Notifications.DeleteByIdAsync(id) > 0;

    private static string Validate(NotificationEditRequest r)
    {
        var text = r.Notification?.Trim() ?? "";
        if (text.Length == 0) throw ValidationFailedException.For("notification", "Notification is required");
        if (text.Length > NotificationFieldLimits.MaxNotificationLength)
            throw ValidationFailedException.For("notification", $"Notification cannot exceed {NotificationFieldLimits.MaxNotificationLength} characters");
        return text;
    }

    private static NotificationDetail ToDetail(AppNotification n) => new()
    {
        Id = n.AppNotificationId, RowVersion = Convert.ToBase64String(n.Version), Notification = n.Notification,
        CreatedOn = n.CreatedOn, UpdatedOn = n.UpdatedOn
    };
}
