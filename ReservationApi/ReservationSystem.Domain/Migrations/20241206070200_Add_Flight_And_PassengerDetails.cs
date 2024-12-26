using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Add_Flight_And_PassengerDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_info",
                columns: table => new
                {
                    FlightId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    flight_number = table.Column<string>(type: "text", nullable: true),
                    departure = table.Column<string>(type: "text", nullable: true),
                    destination = table.Column<string>(type: "text", nullable: true),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cabin_class = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_info", x => x.FlightId);
                });

            migrationBuilder.CreateTable(
                name: "passenger_info",
                columns: table => new
                {
                    PassengerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<string>(type: "text", nullable: true),
                    FlightId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passenger_info", x => x.PassengerId);
                    table.ForeignKey(
                        name: "FK_passenger_info_flight_info_FlightId",
                        column: x => x.FlightId,
                        principalTable: "flight_info",
                        principalColumn: "FlightId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_passenger_info_FlightId",
                table: "passenger_info",
                column: "FlightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "passenger_info");

            migrationBuilder.DropTable(
                name: "flight_info");
        }
    }
}
