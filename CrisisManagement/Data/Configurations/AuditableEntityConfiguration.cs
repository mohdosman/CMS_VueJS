using CrisisManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations;

/// <summary>
/// Base configuration for entities that implement IAuditableEntity.
/// Configures common audit properties: CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Version
/// </summary>
/// <typeparam name="TEntity">Entity type that implements IAuditableEntity</typeparam>
public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ConfigureAuditProperties(builder);
    }

    /// <summary>
    /// Configures the standard audit properties for the entity
    /// </summary>
    protected void ConfigureAuditProperties(EntityTypeBuilder<TEntity> builder)
    {
        // User tracking
        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.UpdatedBy)
            .IsRequired();

        // Timestamp tracking
        builder.Property(e => e.CreatedOn)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedOn)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("(getdate())");

        // Concurrency token
        builder.Property(e => e.Version)
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}