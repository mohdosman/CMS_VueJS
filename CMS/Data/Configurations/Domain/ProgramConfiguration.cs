using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ProgramConfiguration : AuditableEntityConfiguration<CMS.Data.Models.Domain.Program>
{
    public override void Configure(EntityTypeBuilder<CMS.Data.Models.Domain.Program> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ProgramId);

        entity.ToTable("CMS_Program", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EdisonAccountNumber)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.EdisonCategoryNumber)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.EdisonDepartmentNumber)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.ProgramCode)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.ProgramDescription)
            .HasMaxLength(255)
            .IsUnicode(false);

    }
}

