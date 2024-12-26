using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Update_FlightInfo_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "amadeus_session_id",
                table: "flight_info",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_on",
                table: "flight_info",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "flight_offer",
                table: "flight_info",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amadeus_session_id",
                table: "flight_info");

            migrationBuilder.DropColumn(
                name: "created_on",
                table: "flight_info");

            migrationBuilder.DropColumn(
                name: "flight_offer",
                table: "flight_info");
        }
    }
}
