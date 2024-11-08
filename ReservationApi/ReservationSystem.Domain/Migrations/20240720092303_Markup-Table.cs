using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MarkupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_markup",
                columns: table => new
                {
                    markup_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adult_markup = table.Column<decimal>(type: "numeric", nullable: true),
                    child_markup = table.Column<decimal>(type: "numeric", nullable: true),
                    infant_markup = table.Column<decimal>(type: "numeric", nullable: true),
                    apply_markup = table.Column<bool>(type: "boolean", nullable: true),
                    airline = table.Column<string>(type: "text", nullable: true),
                    discount_on_airline = table.Column<decimal>(type: "numeric", nullable: true),
                    apply_airline_discount = table.Column<bool>(type: "boolean", nullable: true),
                    meta = table.Column<string>(type: "text", nullable: true),
                    discount_on_meta = table.Column<decimal>(type: "numeric", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_markup", x => x.markup_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_markup");
        }
    }
}
