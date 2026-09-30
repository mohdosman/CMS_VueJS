using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ServiceFileImportConfiguration : AuditableEntityConfiguration<ServiceFileImport>
{
    public override void Configure(EntityTypeBuilder<ServiceFileImport> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_ServiceFileImport", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.County)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.DOB)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.DOSAdmitDate)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.DischargeDate)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.DurationHours)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.FirstName)
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.Gender)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.LastName)
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.PayorSource)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.PrimaryInsurer)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.ProviderPatientNo)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.SSN)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.ServiceCode)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.ServiceCounty)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.HasOne(d => d.ServiceFile).WithMany(p => p.ServiceFileImports)
            .HasForeignKey(d => d.ServiceFileId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ServiceFileImport_CMS_ServiceFile");

    }
}

