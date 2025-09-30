using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Database.EntityFramework.Models;
using Database.Repository.CoordinateRepo;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Rest_API.Services.Temp;

/// <summary>
/// Provides operations related to the location for outside temperature data,
/// e.g., retrieving coordinates for a given postal code – now via local DB lookup (no external API).
/// </summary>
public class TempService : ITempService
{
    private readonly ICoordinateRepo _coordinateRepo;
    private readonly ILogger<TempService> _logger;

    public TempService(ILogger<TempService> logger, ICoordinateRepo coordinateRepo)
    {
        _logger = logger;
        _coordinateRepo = coordinateRepo;
    }

    /// <summary>
    /// Resolves coordinates (and city name) for a postal code from the local PostgreSQL DB.
    /// First checks CoordinateMappings, then falls back to public.location.
    /// </summary>
    private async Task<(double lat, double lon, string city)?> QueryLatLonByPlzAsync(int postalCode)
    {
        // 1) Check if we already have a mapping stored
        // NOTE: If your repo provides GetByPostalCode(int), prefer that over GetLocation().
        var existing = await _coordinateRepo.GetLocation();
        if (existing is not null && existing.PostalCode == postalCode)
            return (existing.Latitude, existing.Longitude, existing.Location ?? string.Empty);

        // 2) Fallback to the imported public.location table (via Dapper/Npgsql -> pgpool)
        var host = Environment.GetEnvironmentVariable("PGHOST") ?? "pgpool";
        var port = Environment.GetEnvironmentVariable("PGPORT") ?? "9999";
        var user = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
        var pwd  = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
        var db   = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "isopruefi";
        var cs   = $"Host={host};Port={port};Username={user};Password={pwd};Database={db};Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;Timeout=15";

        await using var conn = new NpgsqlConnection(cs);

        const string sql = @"
            SELECT latitude AS Lat, longitude AS Lon, city AS City
            FROM public.location
            WHERE plz = @plz
            -- prefer shorter, 'normal' city names first
            ORDER BY (char_length(city) <= 30) DESC, char_length(city) ASC, city ASC
            LIMIT 1;";

        var row = await conn.QueryFirstOrDefaultAsync<(double Lat, double Lon, string City)?>(sql, new { plz = postalCode.ToString() });
        if (row is null) return null;

        // 3) Persist mapping so the rest of the app can reuse it
        if (!await _coordinateRepo.ExistsPostalCode(postalCode))
        {
            await _coordinateRepo.InsertNewPostalCode(new CoordinateMapping
            {
                PostalCode = postalCode,
                Location   = row.Value.City,
                Latitude   = row.Value.Lat,
                Longitude  = row.Value.Lon,
                LastUsed   = DateTime.UtcNow
            });
        }

        return (row.Value.Lat, row.Value.Lon, row.Value.City);
    }

    /// <inheritdoc />
    public async Task GetCoordinates(int postalCode)
    {
        // Already known?
        if (await _coordinateRepo.ExistsPostalCode(postalCode))
        {
            _logger.LogInformation("There is an existing entry for postal code {PostalCode}.", postalCode);
            return;
        }

        // Local DB lookup (no external HTTP call)
        var coords = await QueryLatLonByPlzAsync(postalCode);
        if (coords is null)
        {
            _logger.LogError("No coordinates found in local DB for postal code {PostalCode}.", postalCode);
            throw new InvalidOperationException("Postal code unknown or not present in dataset.");
        }

        _logger.LogInformation("Coordinates resolved locally and stored: {Lat}, {Lon} ({City})",
            coords.Value.lat, coords.Value.lon, coords.Value.city);
    }

    /// <inheritdoc />
    public async Task<List<Tuple<int, string>>?> ShowAvailableLocations()
    {
        try
        {
            var allPostalcodes = await _coordinateRepo.GetAllLocations();
            return allPostalcodes;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while fetching postal codes from the database");
            return null;
        }
    }
}