using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class SuicideFileImportConfiguration : AuditableEntityConfiguration<SuicideFileImport>
{
    public override void Configure(EntityTypeBuilder<SuicideFileImport> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_SuicideFileImport", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DDOBDay)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDOBMo)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDOBYr)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDODDay)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDODMo)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDODYr)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDeathManner)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DDeathStateCountry)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DNameFirst)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DNameLast)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DNameMiddle)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DResCounty)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DResStateCountry)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DSSN)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DSex)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.DUSArmedForces)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.Provider)
            .HasMaxLength(255)
            .IsUnicode(false);

        entity.HasOne(d => d.SuicideFile).WithMany(p => p.SuicideFileImports)
            .HasForeignKey(d => d.SuicideFileId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_SuicideFileImport_CMS_SuicideFile");

    }
}

