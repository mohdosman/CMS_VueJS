using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class MilitaryStatusConfiguration : AuditableEntityConfiguration<MilitaryStatus>
{
    public override void Configure(EntityTypeBuilder<MilitaryStatus> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.MilitaryStatusId);

        entity.ToTable("CMS_MilitaryStatus", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.MilitaryStatusDescription)
            .HasColumnName("MilitaryStatus")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.MilitaryStatusCode)
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

