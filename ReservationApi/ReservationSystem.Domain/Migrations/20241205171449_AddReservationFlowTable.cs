using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationFlowTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reservation_flow",
                columns: table => new
                {
                    auto_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amadeus_session_id = table.Column<string>(type: "text", nullable: true),
                    request_name = table.Column<string>(type: "text", nullable: true),
                    request = table.Column<string>(type: "jsonb", nullable: true),
                    response = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    is_error = table.Column<bool>(type: "boolean", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_flow", x => x.auto_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_flow");
        }
    }
}
