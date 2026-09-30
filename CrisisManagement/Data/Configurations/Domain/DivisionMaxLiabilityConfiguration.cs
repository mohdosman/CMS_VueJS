using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class DivisionMaxLiabilityConfiguration : AuditableEntityConfiguration<DivisionMaxLiability>
{
    public override void Configure(EntityTypeBuilder<DivisionMaxLiability> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_DivisionMaxLiability", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.MaxLiability).HasColumnType("money");

        entity.HasOne(d => d.FiscalYear).WithMany(p => p.DivisionMaxLiabilities)
            .HasForeignKey(d => d.FiscalYearId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_DivisionMaxLiability_FiscalYearId");

    }
}

