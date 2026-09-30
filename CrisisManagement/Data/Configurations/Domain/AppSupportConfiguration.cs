using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class AppSupportConfiguration : AuditableEntityConfiguration<AppSupport>
{
    public override void Configure(EntityTypeBuilder<AppSupport> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity
            .HasNoKey()
            .ToTable("CMS_AppSupport", DatabaseConstants.DefaultSchema, t =>
    {
        t.UseSqlOutputClause(false);
    });

        entity.Property(e => e.AppSupportId).ValueGeneratedOnAdd();

        entity.HasOne(d => d.Contact).WithMany()
            .HasForeignKey(d => d.ContactId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_AppSupport_CMS_Contact");

    }
}
