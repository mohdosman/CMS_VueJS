using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class ResidentialStatusConfiguration : AuditableEntityConfiguration<ResidentialStatus>
{
    public override void Configure(EntityTypeBuilder<ResidentialStatus> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ResidentialStatusId);

        entity.ToTable("CMS_ResidentialStatus", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.ResidentialStatusDescription)
            .HasColumnName("ResidentialStatus")
            .HasMaxLength(150)
            .IsUnicode(false);

    }
}

