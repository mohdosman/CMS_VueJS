namespace CrisisManagement.Shared.Constants;

/// <summary>
/// Limits for a notification. Unlike the other field limit constants this one is not a column
/// length: CMS_AppNotification.Notification is varchar(max), so the database imposes nothing. The
/// cap is a product rule that the edit form has always applied and the server never did.
/// </summary>
public static class NotificationFieldLimits
{
    /// <summary>
    /// Notification text is shown in a banner, so it has to stay short enough to read there.
    /// </summary>
    public const int MaxNotificationLength = 1000;
}
