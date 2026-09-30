using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class EmploymentStatusConfiguration : AuditableEntityConfiguration<EmploymentStatus>
{
    public override void Configure(EntityTypeBuilder<EmploymentStatus> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.EmploymentStatusId);

        entity.ToTable("CMS_EmploymentStatus", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EmploymentStatusDescription)
            .HasColumnName("EmploymentStatus")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

