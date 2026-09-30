using CMS.Data.Constants;
using CMS.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Identity;

public sealed class PasswordChangeLogConfiguration : AuditableEntityConfiguration<PasswordChangeLog>
{
    public override void Configure(EntityTypeBuilder<PasswordChangeLog> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_PasswordChangeLog", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Indexes
        builder.HasIndex(e => e.UserId);

        // Relationships
        builder.HasOne(d => d.User)
            .WithMany(p => p.PasswordChangeLogs)
            .HasForeignKey(d => d.UserId);
    }
}
