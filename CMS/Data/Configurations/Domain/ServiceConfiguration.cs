using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ServiceConfiguration : AuditableEntityConfiguration<Service>
{
    public override void Configure(EntityTypeBuilder<Service> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_Service", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DOSAdmitDate).HasColumnType("datetime");
        entity.Property(e => e.DischargeDate).HasColumnType("datetime");
        entity.Property(e => e.SessionId)
            .HasMaxLength(250)
            .IsUnicode(false);

        entity.HasOne(d => d.County).WithMany(p => p.ServiceCounties)
            .HasForeignKey(d => d.CountyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Service_CMS_County");

        entity.HasOne(d => d.Patient).WithMany(p => p.Services)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Service_CMS_Patient");

        entity.HasOne(d => d.PayorSource).WithMany(p => p.ServicePayorSources)
            .HasForeignKey(d => d.PayorSourceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Service_CMS_PayorSource");

        entity.HasOne(d => d.PrimaryInsurer).WithMany(p => p.ServicePrimaryInsurers)
            .HasForeignKey(d => d.PrimaryInsurerId)
            .HasConstraintName("FK_CMS_Service_CMS_PayorSource1");

        entity.HasOne(d => d.Provider).WithMany(p => p.Services)
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Service_CMS_Provider");

        entity.HasOne(d => d.ServiceCode).WithMany(p => p.Services)
            .HasForeignKey(d => d.ServiceCodeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Service_CMS_ServiceCode");

        entity.HasOne(d => d.ServiceCounty).WithMany(p => p.ServiceServiceCounties)
            .HasForeignKey(d => d.ServiceCountyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Service_CMS_County1");

        entity.HasOne(d => d.ServiceFileImport).WithMany(p => p.Services)
            .HasForeignKey(d => d.ServiceFileImportId)
            .HasConstraintName("FK_CMS_Service_CMS_ServiceFileImport");

    }
}

