using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ContractConfiguration : AuditableEntityConfiguration<Contract>
{
    public override void Configure(EntityTypeBuilder<Contract> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ContractId);

        entity.ToTable("CMS_Contract", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.ContractNumber)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.LineNumber)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.Name)
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.StartupPaidToDate).HasColumnType("datetime");

        entity.HasOne(d => d.FiscalYear).WithMany(p => p.Contracts)
            .HasForeignKey(d => d.FiscalYearId)
            .HasConstraintName("FK_Contract_FiscalYearID");

        entity.HasOne(d => d.Program).WithMany(p => p.Contracts)
            .HasForeignKey(d => d.ProgramId)
            .HasConstraintName("FK_Contract_ProgramId");

        entity.HasOne(d => d.Provider).WithMany(p => p.Contracts)
            .HasForeignKey(d => d.ProviderId)
            .HasConstraintName("FK_Contract_ProviderID");

    }
}

