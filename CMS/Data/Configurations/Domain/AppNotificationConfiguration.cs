using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class AppNotificationConfiguration : AuditableEntityConfiguration<AppNotification>
{
    public override void Configure(EntityTypeBuilder<AppNotification> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.HasKey(e => e.AppNotificationId);

        entity
            .ToTable("CMS_AppNotification", DatabaseConstants.DefaultSchema, t =>
    {
        t.UseSqlOutputClause(false);
    });

        entity.Property(e => e.AppNotificationId).ValueGeneratedOnAdd();
        entity.Property(e => e.Notification).IsUnicode(false);

    }
}
