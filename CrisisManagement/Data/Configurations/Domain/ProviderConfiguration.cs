using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public sealed class ProviderConfiguration : AuditableEntityConfiguration<Provider>
{
    public override void Configure(EntityTypeBuilder<Provider> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        // Table configuration
        entity.ToTable("CMS_Provider", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasKey(x => x.ProviderId);

        // Entity-specific properties
        entity.Property(x => x.Name).IsRequired().HasMaxLength(ProviderFieldLimits.MaxNameLength);
        entity.Property(x => x.Abbreviation).HasMaxLength(ProviderFieldLimits.MaxAbbreviationLength);
        entity.Property(x => x.EdisonNumber).HasMaxLength(ProviderFieldLimits.MaxEdisonNumberLength);
        entity.Property(x => x.Npi).HasMaxLength(ProviderFieldLimits.MaxNpiColumnLength);

        // Uniqueness constraints
        entity.HasIndex(x => x.Npi)
            .IsUnique()
            .HasFilter("[Npi] IS NOT NULL");
        
        entity.HasIndex(x => x.EdisonNumber)
            .IsUnique()
            .HasFilter("[EdisonNumber] IS NOT NULL");
    }
}
