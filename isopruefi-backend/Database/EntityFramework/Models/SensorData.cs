using System.ComponentModel.DataAnnotations;

namespace Database.EntityFramework.Models;

/// <summary>
///     Model for storing sensor temperature data in PostgreSQL.
/// </summary>
public class SensorData
{
    /// <summary>
    ///     Primary key for the sensor data entry.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    ///     Temperature measurement value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    ///     Sensor identifier.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Sensor { get; set; } = null!;

    /// <summary>
    ///     Unix timestamp of the measurement.
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    ///     DateTime representation of the timestamp for easier querying.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     Sequence number of the measurement.
    /// </summary>
    public int Sequence { get; set; }
}