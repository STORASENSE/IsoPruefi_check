using System.ComponentModel.DataAnnotations;

namespace Database.EntityFramework.Models;

/// <summary>
///     Model for storing Arduino uptime data in PostgreSQL.
/// </summary>
public class UptimeData
{
    /// <summary>
    ///     Primary key for the uptime data entry.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    ///     Sensor name/identifier.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Sensor { get; set; } = null!;

    /// <summary>
    ///     Unix timestamp when the Arduino was available.
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    ///     DateTime representation of the timestamp for easier querying.
    /// </summary>
    public DateTime DateTime { get; set; }
}