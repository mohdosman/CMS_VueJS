using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Models.Auth;

/// <summary>
/// Represents a user authentication session stored in the database.
/// </summary>
public sealed class AppSessionEntity
{
    /// <summary>
    /// Unique session identifier (stored in cookie).
    /// </summary>
    [Key]
    [MaxLength(32)]
    public required string Id { get; init; }

    /// <summary>
    /// User ID associated with this session.
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// When the session was created.
    /// </summary>
    public required DateTimeOffset CreatedUtc { get; init; }

    /// <summary>
    /// Absolute expiration time (cannot be extended beyond this).
    /// </summary>
    public required DateTimeOffset AbsoluteExpiresUtc { get; init; }

    /// <summary>
    /// Last activity timestamp (updated on each request).
    /// </summary>
    public required DateTimeOffset LastActivityUtc { get; set; }
}