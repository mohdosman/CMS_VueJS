using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity;

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
