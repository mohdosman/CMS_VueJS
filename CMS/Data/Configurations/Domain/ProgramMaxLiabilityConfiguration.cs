using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ProgramMaxLiabilityConfiguration : AuditableEntityConfiguration<ProgramMaxLiability>
{
    public override void Configure(EntityTypeBuilder<ProgramMaxLiability> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_ProgramMaxLiability", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.MaxLiability).HasColumnType("money");

        entity.HasOne(d => d.FiscalYear).WithMany(p => p.ProgramMaxLiabilities)
            .HasForeignKey(d => d.FiscalYearId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ProgramMaxLiability_FiscalYear");

        entity.HasOne(d => d.Program).WithMany(p => p.ProgramMaxLiabilities)
            .HasForeignKey(d => d.ProgramId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ProgramMaxLiability_Program");

    }
}

