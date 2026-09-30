using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class PrimaryProblemConfiguration : AuditableEntityConfiguration<PrimaryProblem>
{
    public override void Configure(EntityTypeBuilder<PrimaryProblem> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_PrimaryProblem", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.PrimaryProblemDescription)
            .HasColumnName("PrimaryProblem")
            .HasMaxLength(150)
            .IsUnicode(false);

    }
}

