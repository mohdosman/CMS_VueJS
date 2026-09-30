namespace CrisisManagement.Features.Notifications.ViewModels;

public sealed class NotificationSearchRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record NotificationListItem(int Id, string Notification, DateTime CreatedOn);

public sealed class NotificationDetail
{
    public int Id { get; set; }
    public string RowVersion { get; set; } = "";
    public string Notification { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

// Create and update share one body.
public sealed class NotificationEditRequest
{
    public string? RowVersion { get; set; }
    public string? Notification { get; set; }
}
