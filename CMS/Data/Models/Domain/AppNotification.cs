using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class AppNotification : AuditableEntity
{
    public int AppNotificationId { get; set; }

    public string Notification { get; set; } = null!;

}

