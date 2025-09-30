using System.Text.RegularExpressions;
using Database.EntityFramework;
using Database.Repository.CoordinateRepo;
using Database.Repository.SettingsRepo;
using Database.Repository.TimeDataRepo;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTT_Receiver_Worker.MQTT;
using MQTT_Receiver_Worker.MQTT.Interfaces;
using Rest_API;
using Testcontainers.PostgreSql;

namespace LoadTests.Infrastructure;

/// <summary>
///     Web application factory for REST API load tests using TestContainers
/// </summary>
public class LoadTestRestAPIFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer;

    /// <summary>
    ///     Initializes a new instance of the LoadTestRestAPIFactory
    /// </summary>
    public LoadTestRestAPIFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:alpine3.21")
            .WithDatabase("isopruefi_loadtest")
            .WithUsername("loadtest")
            .WithPassword("LoadTest123!")
            .Build();
    }

    /// <summary>
    ///     Gets the PostgreSQL database connection string
    /// </summary>
    public string DatabaseConnectionString => _dbContainer.GetConnectionString();

    /// <summary>
    ///     Configures the web host for REST API load testing
    /// </summary>
    /// <param name="builder">Web host builder to configure</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add configuration for load testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:UserName"] = "admin",
                ["Admin:Email"] = "loadtestadmin@loadtest.com",
                ["Admin:Password"] = "LoadTestAdmin123!",
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Jwt:ValidAudience"] = "localhost",
                ["Jwt:ValidIssuer"] = "localhost",
                ["Jwt:Secret"] = "your-secure-256-bit-secret-key-replace-this-in-production"
            });

            config.AddJsonFile("appsettings.loadtest.json", true);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Configure logging to reduce noise during load testing
            services.Configure<LoggerFilterOptions>(options => { options.MinLevel = LogLevel.Information; });

            // Register required services for load testing
            services.AddMemoryCache();
            services.AddScoped<ITimeDataRepo, TimeDataRepo>();
            services.AddScoped<ISettingsRepo, SettingsRepo>();
            services.AddSingleton<IReceiver, Receiver>();
            services.AddSingleton<IConnection, Connection>();
            services.AddScoped<ICoordinateRepo, CoordinateRepo>();
            services.AddHttpClient();
        });

        // Disable HTTPS redirection for load tests
        builder.UseSetting("HTTPS_REDIRECTION", "false");
        builder.UseEnvironment("LoadTesting");
    }

    /// <summary>
    ///     Initialize database container and seed test data
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start database container
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        ApplicationDbContext.ApplyMigration<ApplicationDbContext>(scope);
    }

    /// <summary>
    ///     Clean up container
    /// </summary>
    public async Task CleanupAsync()
    {
        try
        {
            if (_dbContainer != null) 
                await _dbContainer.StopAsync();
        }
        catch (ObjectDisposedException)
        {
            // Container already disposed, ignore
        }
    }

    /// <summary>
    ///     Disposes of container resources
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            try
            {
                CleanupAsync().GetAwaiter().GetResult();
                _dbContainer?.DisposeAsync().GetAwaiter().GetResult();
            }
            catch (ObjectDisposedException)
            {
                // Container already disposed, ignore
            }

        base.Dispose(disposing);
    }
}