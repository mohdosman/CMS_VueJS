using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class DispositionConfiguration : AuditableEntityConfiguration<Disposition>
{
    public override void Configure(EntityTypeBuilder<Disposition> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.DispositionId);

        entity.ToTable("CMS_Disposition", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DispositionDescription)
            .HasColumnName("Disposition")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.DispositionCode)
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

