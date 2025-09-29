using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPostgreSQLTablesForInfluxData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutsideWeatherData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Place = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    TemperatureFahrenheit = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostalCode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutsideWeatherData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Sensor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UptimeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sensor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UptimeData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutsideWeatherData_Place_Timestamp",
                table: "OutsideWeatherData",
                columns: new[] { "Place", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OutsideWeatherData_Timestamp",
                table: "OutsideWeatherData",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SensorData_DateTime",
                table: "SensorData",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_SensorData_Sensor_DateTime",
                table: "SensorData",
                columns: new[] { "Sensor", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_UptimeData_DateTime",
                table: "UptimeData",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_UptimeData_Sensor_DateTime",
                table: "UptimeData",
                columns: new[] { "Sensor", "DateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutsideWeatherData");

            migrationBuilder.DropTable(
                name: "SensorData");

            migrationBuilder.DropTable(
                name: "UptimeData");
        }
    }
}
