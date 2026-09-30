using CMS.Data.Constants;
using CMS.Shared.Constants;
using CMS.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Identity;


public sealed class ApplicationUserConfiguration : AuditableEntityConfiguration<ApplicationUser>
{
    public override void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        base.Configure(builder);

        builder.ToTable("RBS_User", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("UserId")
            .ValueGeneratedOnAdd();

        builder.Ignore(e => e.FullName);
        builder.Ignore(e => e.IsLockedOut);

        builder.Property(e => e.UserKey)
            .HasColumnName("UserKey")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("(newid())");

        builder.Property(e => e.UserName)
            .HasColumnName("UserName")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.NormalizedUserName)
            .HasColumnName("NormalizedUserName")
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasColumnName("Email")
            .HasColumnType($"varchar({UserFieldLimits.MaxEmailLength})")
            .IsRequired();

        builder.Property(e => e.NormalizedEmail)
            .HasColumnName("NormalizedEmail")
            .HasColumnType($"varchar({UserFieldLimits.MaxEmailLength})");

        builder.Property(e => e.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(450);

        builder.Property(e => e.SecurityStamp)
            .HasColumnName("SecurityStamp")
            .HasMaxLength(64);

        builder.Property(e => e.ConcurrencyStamp)
            .HasColumnName("ConcurrencyStamp")
            .HasMaxLength(64);

        builder.Property(e => e.PhoneNumber)
            .HasColumnName("PhoneNumber")
            .HasMaxLength(32);

        builder.Property(e => e.EmailConfirmed)
            .HasColumnName("EmailConfirmed")
            .IsRequired();

        builder.Property(e => e.PhoneNumberConfirmed)
            .HasColumnName("PhoneNumberConfirmed")
            .IsRequired();

        builder.Property(e => e.TwoFactorEnabled)
            .HasColumnName("TwoFactorEnabled")
            .IsRequired();

        builder.Property(e => e.LockoutEnd)
            .HasColumnName("LockoutEnd");

        builder.Property(e => e.LockoutEnabled)
            .HasColumnName("LockoutEnabled")
            .IsRequired();

        builder.Property(e => e.AccessFailedCount)
            .HasColumnName("AccessFailedCount")
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(UserFieldLimits.MaxNameLength)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(UserFieldLimits.MaxNameLength)
            .IsRequired();

        builder.Property(e => e.Comment)
            .HasColumnName("Comment")
            .HasColumnType("varchar(max)");

        builder.Property(e => e.IsADAccount)
            .HasColumnName("IsADAccount")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(e => e.LastPasswordChangedDate)
            .HasColumnName("LastPasswordChangedDate")
            .HasColumnType("datetime")
            .HasDefaultValueSql("(getdate())")
            .IsRequired();

        builder.Property(e => e.LegacyPassword)
            .HasColumnName("Password")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.LegacyIsLockedOut)
            .HasColumnName("IsLockedOut")
            .IsRequired();

        builder.Property(e => e.IsTemporaryPassword)
            .HasColumnName("IsTemporaryPassword")
            .IsRequired();

        builder.Property(e => e.LastLoginDate)
            .HasColumnName("LastLoginDate")
            .HasColumnType("datetime")
            .HasDefaultValueSql("'17530101'")
            .IsRequired();

        builder.Property(e => e.LastLockoutDate)
            .HasColumnName("LastLockoutDate")
            .HasColumnType("datetime")
            .HasDefaultValueSql("'17530101'")
            .IsRequired();

        builder.Property(e => e.FailedPasswordAttemptCount)
            .HasColumnName("FailedPasswordAttemptCount")
            .IsRequired();

        builder.Property(e => e.Version)
            .HasColumnName("Version")
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(e => e.NormalizedUserName)
            .HasDatabaseName("IX_RBS_User_NormalizedUserName")
            .IsUnique()
            .HasFilter("[NormalizedUserName] IS NOT NULL");

        builder.HasIndex(e => e.NormalizedEmail)
            .HasDatabaseName("IX_RBS_User_NormalizedEmail")
            .HasFilter("[NormalizedEmail] IS NOT NULL");

        builder.HasIndex(e => e.UserKey)
            .HasDatabaseName("IX_RBS_User_UserKey")
            .IsUnique();

        builder.HasMany(u => u.Claims)
            .WithOne()
            .HasForeignKey(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Roles)
            .WithOne()
            .HasForeignKey(r => r.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Logons)
            .WithOne()
            .HasForeignKey("UserId");

        builder.HasMany(u => u.PasswordChangeLogs)
            .WithOne()
            .HasForeignKey("UserId");
    }
}
