using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarkupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "airline_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "between_hours",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "between_hours_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "end_airport",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "end_airport_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fare_type",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "fare_type_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gds",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "gds_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_percentage",
                table: "flight_markup",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "journy_type",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "journy_type_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marketing_source",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "marketing_source_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "meta_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "start_airport",
                table: "flight_markup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "start_airport_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "airline_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "between_hours",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "between_hours_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "end_airport",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "end_airport_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "fare_type",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "fare_type_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "gds",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "gds_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "is_percentage",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "journy_type",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "journy_type_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "marketing_source",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "marketing_source_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "meta_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "start_airport",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "start_airport_markup",
                table: "flight_markup");
        }
    }
}
