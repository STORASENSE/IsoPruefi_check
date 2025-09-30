using LoadTests.Infrastructure;

namespace LoadTests.Tests;

/// <summary>
///     Basic infrastructure test to verify TestContainers and factory setup
/// </summary>
[TestFixture]
public class BasicInfrastructureTest : LoadTestBase
{
    /// <summary>
    ///     Tests that all infrastructure components are properly initialized
    /// </summary>
    [Test]
    public void Test_Infrastructure_Setup()
    {
        // Test that all components are initialized
        Assert.That(ApiFactory, Is.Not.Null, "Factory should be initialized");
        Assert.That(ApiClient, Is.Not.Null, "ApiClient should be initialized");

        // Test database connection
        Assert.That(ApiFactory.DatabaseConnectionString, Is.Not.Empty,
            "Database connection string should not be empty");

        // Test PostgreSQL setup (database connection string is already tested above)

        // Test MQTT setup
        Assert.That(MqttFactory.MqttPort, Is.GreaterThan(0), "MQTT port should be set");

        // Test API client base address
        var baseUrl = GetApiBaseUrl();
        Assert.That(baseUrl, Is.Not.Empty, "API base URL should not be empty");
    }
}