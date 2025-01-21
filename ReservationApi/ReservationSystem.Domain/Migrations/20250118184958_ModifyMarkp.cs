using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ModifyMarkp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "date_markup",
                table: "flight_markup",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "from_date",
                table: "flight_markup",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "to_date",
                table: "flight_markup",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_markup",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "from_date",
                table: "flight_markup");

            migrationBuilder.DropColumn(
                name: "to_date",
                table: "flight_markup");
        }
    }
}
