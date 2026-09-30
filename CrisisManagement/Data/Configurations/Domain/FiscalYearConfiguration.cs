using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FiscalYearConfiguration : AuditableEntityConfiguration<FiscalYear>
{
    public override void Configure(EntityTypeBuilder<FiscalYear> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.FiscalYearId);

        entity.ToTable("CMS_FiscalYear", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EndDate).HasColumnType("datetime");
        entity.Property(e => e.StartDate).HasColumnType("datetime");
        entity.Property(e => e.FiscalYearDescription)
            .HasColumnName("FiscalYear");

    }
}

