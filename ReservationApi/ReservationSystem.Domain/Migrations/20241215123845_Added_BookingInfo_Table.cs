using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Added_BookingInfo_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "passenger_info",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsLead",
                table: "passenger_info",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "passenger_type",
                table: "passenger_info",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_id",
                table: "passenger_info",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "booking_info",
                columns: table => new
                {
                    auto_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pnr_number = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    session_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_info", x => x.auto_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_info");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "passenger_info");

            migrationBuilder.DropColumn(
                name: "IsLead",
                table: "passenger_info");

            migrationBuilder.DropColumn(
                name: "passenger_type",
                table: "passenger_info");

            migrationBuilder.DropColumn(
                name: "session_id",
                table: "passenger_info");
        }
    }
}
