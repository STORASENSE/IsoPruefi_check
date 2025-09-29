using System.ComponentModel.DataAnnotations;

namespace Database.EntityFramework.Models;

/// <summary>
///     Model for storing outside weather data in PostgreSQL.
/// </summary>
public class OutsideWeatherData
{
    /// <summary>
    ///     Primary key for the outside weather data entry.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    ///     Name of the place/city.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Place { get; set; } = null!;

    /// <summary>
    ///     Name of the website/API that provided the data.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Website { get; set; } = null!;

    /// <summary>
    ///     Temperature value in Celsius.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    ///     Temperature value in Fahrenheit.
    /// </summary>
    public double TemperatureFahrenheit { get; set; }

    /// <summary>
    ///     Timestamp of the weather data.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Associated postal code.
    /// </summary>
    public int PostalCode { get; set; }
}