using CMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations;

public sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> b)
    {
        b.ToTable("CMS_Provider", "dbo");
        b.HasKey(e => e.ProviderId);
    }
}

public sealed class ProviderUserConfiguration : IEntityTypeConfiguration<ProviderUser>
{
    public void Configure(EntityTypeBuilder<ProviderUser> b)
    {
        b.ToTable("CMS_ProviderUser", "dbo", t => t.UseSqlOutputClause(false));
        b.HasKey(e => e.ProviderUserId);
        b.Property(e => e.CreatedOn).HasColumnType("datetime");
        b.Property(e => e.UpdatedOn).HasColumnType("datetime");
    }
}

public sealed class LogonConfiguration : IEntityTypeConfiguration<Logon>
{
    public void Configure(EntityTypeBuilder<Logon> b)
    {
        b.ToTable("RBS_Logon", "dbo");
        b.HasKey(e => e.LogonId);
        b.Property(e => e.LogOnDateTime).HasColumnType("datetime");
    }
}

public sealed class PasswordChangeLogConfiguration : IEntityTypeConfiguration<PasswordChangeLog>
{
    public void Configure(EntityTypeBuilder<PasswordChangeLog> b)
    {
        b.ToTable("RBS_PasswordChangeLog", "dbo", t => t.UseSqlOutputClause(false));
        b.HasKey(e => e.PasswordChangeLogId);
        b.Property(e => e.CreatedOn).HasColumnType("datetime");
        b.Property(e => e.UpdatedOn).HasColumnType("datetime");
    }
}

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.ToTable("CMS_Document", "dbo");
        b.HasKey(e => e.DocumentId);
        b.Property(e => e.CreatedOn).HasColumnType("datetime");
    }
}
