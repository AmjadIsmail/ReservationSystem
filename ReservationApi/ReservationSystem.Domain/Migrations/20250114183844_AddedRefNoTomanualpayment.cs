using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddedRefNoTomanualpayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "booking_ref",
                table: "manual_payment",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "booking_ref",
                table: "manual_payment");
        }
    }
}
