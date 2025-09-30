namespace Database.EntityFramework.Models;

/// <summary>
///     Represents uptime data point for API responses.
/// </summary>
public class UptimeDataPoint
{
    /// <summary>
    ///     Sensor name/identifier.
    /// </summary>
    public string Sensor { get; set; } = null!;

    /// <summary>
    ///     DateTime when the Arduino was available.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     Unix timestamp when the Arduino was available.
    /// </summary>
    public long Timestamp { get; set; }
}