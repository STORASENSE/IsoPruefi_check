using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace Rest_API.Services.Locations;

public sealed class LocationService : ILocationService
{
    private readonly string _cs;

    public LocationService(IConfiguration cfg)
    {
        var host = Environment.GetEnvironmentVariable("PGHOST") ?? "pgpool";
        var port = Environment.GetEnvironmentVariable("PGPORT") ?? "9999";
        var user = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
        var pwd  = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
        var db   = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "isopruefi";
        _cs = $"Host={host};Port={port};Username={user};Password={pwd};Database={db};Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;Timeout=15";
    }

    private NpgsqlConnection Open() => new(_cs);

    public async Task<IEnumerable<string>> GetCitiesByPlzAsync(string plz, bool onlyShortNames = true)
    {
        const string sqlShort = @"
SELECT DISTINCT city
FROM public.location
WHERE plz = @plz
  AND char_length(city) <= 30
ORDER BY city;";
        const string sqlAll = @"
SELECT DISTINCT city
FROM public.location
WHERE plz = @plz
ORDER BY city;";

        await using var c = Open();
        return await c.QueryAsync<string>(onlyShortNames ? sqlShort : sqlAll, new { plz });
    }
}