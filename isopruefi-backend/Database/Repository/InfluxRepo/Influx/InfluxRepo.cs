using Database.EntityFramework;
using Database.EntityFramework.Models;
using InfluxDB3.Client.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repository.InfluxRepo.Influx;

/// <inheritdoc />
public class InfluxRepo : IInfluxRepo
{
    /// <summary>
    ///     Database context for accessing PostgreSQL.
    /// </summary>
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    ///     The logger instance used to record diagnostic information.
    /// </summary>
    private readonly ILogger<InfluxRepo> _logger;
    
    /// <summary>
    ///     Constructor for the InfluxRepo class.
    /// </summary>
    /// <param name="context">Database context for PostgreSQL operations.</param>
    /// <param name="logger">Logger instance for capturing diagnostics.</param>
    public InfluxRepo(ApplicationDbContext context, ILogger<InfluxRepo> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task WriteSensorData(double measurement, string sensor, long timestamp, int sequence)
    {
        try
        {
            var dateTimeUtc = DateTimeOffset
                .FromUnixTimeSeconds(timestamp)
                .UtcDateTime;

            var sensorData = new SensorData
            {
                Value = measurement,
                Sensor = sensor,
                Timestamp = timestamp,
                DateTime = dateTimeUtc,
                Sequence = sequence
            };

            _context.SensorData.Add(sensorData);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error writing sensor data to PostgreSQL");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WriteOutsideWeatherData(string place, string website, double temperature, DateTime timestamp,
        int postalcode)
    {
        try
        {
            var outsideWeatherData = new OutsideWeatherData
            {
                Place = place,
                Website = website,
                Temperature = temperature,
                TemperatureFahrenheit = temperature * 9 / 5 + 32,
                Timestamp = timestamp,
                PostalCode = postalcode
            };

            _context.OutsideWeatherData.Add(outsideWeatherData);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error writing outside weather data to PostgreSQL");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WriteUptime(string sensor, long timestamp)
    {
        try
        {
            var dateTimeUtc = DateTimeOffset
                .FromUnixTimeSeconds(timestamp)
                .UtcDateTime;

            var uptimeData = new UptimeData
            {
                Sensor = sensor,
                Timestamp = timestamp,
                DateTime = dateTimeUtc
            };

            _context.UptimeData.Add(uptimeData);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error writing uptime data to PostgreSQL");
            throw;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<object?[]> GetOutsideWeatherData(DateTime start, DateTime end, string place)
    {
        var timespan = end - start;
        List<OutsideWeatherData> data;
        
        try
        {
            var query = _context.OutsideWeatherData
                .Where(x => x.Place == place && x.Timestamp >= start && x.Timestamp <= end)
                .OrderBy(x => x.Timestamp);

            data = await query.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving outside weather data from PostgreSQL");
            throw;
        }

        IEnumerable<object?[]> grouped;

        if (timespan.TotalHours < 24)
        {
            grouped = data
                .GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day, x.Timestamp.Hour, x.Timestamp.Minute, 0))
                .Select(g => new object?[] { g.Key, g.Average(x => x.Temperature) });
        }
        else if (timespan.TotalDays < 30)
        {
            grouped = data
                .GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day, x.Timestamp.Hour, 0, 0))
                .Select(g => new object?[] { g.Key, g.Average(x => x.Temperature) });
        }
        else
        {
            grouped = data
                .GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day))
                .Select(g => new object?[] { g.Key, g.Average(x => x.Temperature) });
        }

        foreach (var item in grouped)
            yield return item;
    }
    
    /// <inheritdoc />
    public async IAsyncEnumerable<object?[]> GetSensorWeatherData(DateTime start, DateTime end, string sensor)
    {
        var timespan = end - start;
        List<SensorData> data;
        
        try
        {
            var query = _context.SensorData
                .Where(x => x.Sensor == sensor && x.DateTime >= start && x.DateTime <= end)
                .OrderBy(x => x.DateTime);

            data = await query.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving sensor weather data from PostgreSQL");
            throw;
        }

        IEnumerable<object?[]> grouped;

        if (timespan.TotalHours < 24)
        {
            grouped = data
                .GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, x.DateTime.Day, x.DateTime.Hour, x.DateTime.Minute, 0))
                .Select(g => new object?[] { g.Key, g.Average(x => x.Value) });
        }
        else if (timespan.TotalDays < 30)
        {
            grouped = data
                .GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, x.DateTime.Day, x.DateTime.Hour, 0, 0))
                .Select(g => new object?[] { g.Key, g.Average(x => x.Value) });
        }
        else
        {
            grouped = data
                .GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, x.DateTime.Day))
                .Select(g => new object?[] { g.Key, g.Average(x => x.Value) });
        }

        foreach (var item in grouped)
            yield return item;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<PointDataValues> GetUptime(string sensor)
    {
        List<UptimeData> uptimeData;
        
        try
        {
            uptimeData = await _context.UptimeData
                .Where(x => x.Sensor == sensor)
                .OrderBy(x => x.DateTime)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving uptime data from PostgreSQL");
            throw;
        }

        foreach (var item in uptimeData)
        {
            var pointData = new PointDataValues();
            pointData.SetField("sensor", item.Sensor);
            pointData.SetTimestamp(item.DateTime);
            yield return pointData;
        }
    }
}