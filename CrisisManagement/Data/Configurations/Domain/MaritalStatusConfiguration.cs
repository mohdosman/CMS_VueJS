using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class MaritalStatusConfiguration : AuditableEntityConfiguration<MaritalStatus>
{
    public override void Configure(EntityTypeBuilder<MaritalStatus> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.MaritalStatusId);

        entity.ToTable("CMS_MaritalStatus", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.MaritalStatusDescription)
            .HasColumnName("MaritalStatus")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.MaritalStatusCode)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

