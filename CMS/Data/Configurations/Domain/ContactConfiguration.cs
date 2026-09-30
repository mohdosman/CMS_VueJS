using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using CMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ContactConfiguration : AuditableEntityConfiguration<Contact>
{
    public override void Configure(EntityTypeBuilder<Contact> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ContactId);

        entity.ToTable("CMS_Contact", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.EmailAddress)
            .HasMaxLength(ProviderFieldLimits.MaxContactEmailLength)
            .IsUnicode(false);
        entity.Property(e => e.FirstName)
            .HasMaxLength(ProviderFieldLimits.MaxContactNameLength)
            .IsUnicode(false);
        entity.Property(e => e.LastName)
            .HasMaxLength(ProviderFieldLimits.MaxContactNameLength)
            .IsUnicode(false);
        entity.Property(e => e.Phone)
            .HasMaxLength(ProviderFieldLimits.MaxPhoneLength)
            .IsUnicode(false);
        entity.Property(e => e.Title)
            .HasMaxLength(ProviderFieldLimits.MaxContactNameLength)
            .IsUnicode(false);
        entity.Property(e => e.WirelessPhone)
            .HasMaxLength(ProviderFieldLimits.MaxPhoneLength)
            .IsUnicode(false);

    }
}
