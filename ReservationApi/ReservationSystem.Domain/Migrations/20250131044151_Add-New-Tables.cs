using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReservationSystem.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apply_markup",
                columns: table => new
                {
                    markup_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adult_markup = table.Column<decimal>(type: "numeric", nullable: true),
                    child_markup = table.Column<decimal>(type: "numeric", nullable: true),
                    infant_markup = table.Column<decimal>(type: "numeric", nullable: true),
                    is_percentage = table.Column<bool>(type: "boolean", nullable: true),
                    airline = table.Column<string>(type: "text", nullable: true),
                    between_hours_from = table.Column<string>(type: "text", nullable: true),
                    between_hours_to = table.Column<decimal>(type: "numeric", nullable: true),
                    start_airport = table.Column<string>(type: "text", nullable: true),
                    end_airport = table.Column<string>(type: "text", nullable: true),
                    from_date = table.Column<DateOnly>(type: "date", nullable: true),
                    to_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apply_markup", x => x.markup_id);
                });

            migrationBuilder.CreateTable(
                name: "day_name",
                columns: table => new
                {
                    day_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    day_name = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_day_name", x => x.day_id);
                });

            migrationBuilder.CreateTable(
                name: "fare_type",
                columns: table => new
                {
                    faretype_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fare_type_name = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fare_type", x => x.faretype_id);
                });

            migrationBuilder.CreateTable(
                name: "gds",
                columns: table => new
                {
                    gds_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gds_name = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gds", x => x.gds_id);
                });

            migrationBuilder.CreateTable(
                name: "journy_type",
                columns: table => new
                {
                    journytype_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    journytype = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journy_type", x => x.journytype_id);
                });

            migrationBuilder.CreateTable(
                name: "marketing_source",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_name = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketing_source", x => x.source_id);
                });

            migrationBuilder.CreateTable(
                name: "markup_day",
                columns: table => new
                {
                    markup_day_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkupId = table.Column<int>(type: "integer", nullable: true),
                    DayId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_markup_day", x => x.markup_day_id);
                    table.ForeignKey(
                        name: "FK_markup_day_apply_markup_MarkupId",
                        column: x => x.MarkupId,
                        principalTable: "apply_markup",
                        principalColumn: "markup_id");
                    table.ForeignKey(
                        name: "FK_markup_day_day_name_DayId",
                        column: x => x.DayId,
                        principalTable: "day_name",
                        principalColumn: "day_id");
                });

            migrationBuilder.CreateTable(
                name: "MarkupFareTypes",
                columns: table => new
                {
                    markup_fare_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkupId = table.Column<int>(type: "integer", nullable: false),
                    FareTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupFareTypes", x => x.markup_fare_id);
                    table.ForeignKey(
                        name: "FK_MarkupFareTypes_apply_markup_MarkupId",
                        column: x => x.MarkupId,
                        principalTable: "apply_markup",
                        principalColumn: "markup_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkupFareTypes_fare_type_FareTypeId",
                        column: x => x.FareTypeId,
                        principalTable: "fare_type",
                        principalColumn: "faretype_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarkupGds",
                columns: table => new
                {
                    markup_gds_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkupId = table.Column<int>(type: "integer", nullable: false),
                    GdsId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupGds", x => x.markup_gds_id);
                    table.ForeignKey(
                        name: "FK_MarkupGds_apply_markup_MarkupId",
                        column: x => x.MarkupId,
                        principalTable: "apply_markup",
                        principalColumn: "markup_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkupGds_gds_GdsId",
                        column: x => x.GdsId,
                        principalTable: "gds",
                        principalColumn: "gds_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarkupJournyTypes",
                columns: table => new
                {
                    markup_journytype_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkupId = table.Column<int>(type: "integer", nullable: false),
                    JournyTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupJournyTypes", x => x.markup_journytype_id);
                    table.ForeignKey(
                        name: "FK_MarkupJournyTypes_apply_markup_MarkupId",
                        column: x => x.MarkupId,
                        principalTable: "apply_markup",
                        principalColumn: "markup_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkupJournyTypes_journy_type_JournyTypeId",
                        column: x => x.JournyTypeId,
                        principalTable: "journy_type",
                        principalColumn: "journytype_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarkupMarketingSources",
                columns: table => new
                {
                    markup_source_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkupId = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupMarketingSources", x => x.markup_source_id);
                    table.ForeignKey(
                        name: "FK_MarkupMarketingSources_apply_markup_MarkupId",
                        column: x => x.MarkupId,
                        principalTable: "apply_markup",
                        principalColumn: "markup_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkupMarketingSources_marketing_source_SourceId",
                        column: x => x.SourceId,
                        principalTable: "marketing_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_markup_day_DayId",
                table: "markup_day",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_markup_day_MarkupId",
                table: "markup_day",
                column: "MarkupId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupFareTypes_FareTypeId",
                table: "MarkupFareTypes",
                column: "FareTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupFareTypes_MarkupId",
                table: "MarkupFareTypes",
                column: "MarkupId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupGds_GdsId",
                table: "MarkupGds",
                column: "GdsId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupGds_MarkupId",
                table: "MarkupGds",
                column: "MarkupId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupJournyTypes_JournyTypeId",
                table: "MarkupJournyTypes",
                column: "JournyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupJournyTypes_MarkupId",
                table: "MarkupJournyTypes",
                column: "MarkupId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupMarketingSources_MarkupId",
                table: "MarkupMarketingSources",
                column: "MarkupId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupMarketingSources_SourceId",
                table: "MarkupMarketingSources",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "markup_day");

            migrationBuilder.DropTable(
                name: "MarkupFareTypes");

            migrationBuilder.DropTable(
                name: "MarkupGds");

            migrationBuilder.DropTable(
                name: "MarkupJournyTypes");

            migrationBuilder.DropTable(
                name: "MarkupMarketingSources");

            migrationBuilder.DropTable(
                name: "day_name");

            migrationBuilder.DropTable(
                name: "fare_type");

            migrationBuilder.DropTable(
                name: "gds");

            migrationBuilder.DropTable(
                name: "journy_type");

            migrationBuilder.DropTable(
                name: "apply_markup");

            migrationBuilder.DropTable(
                name: "marketing_source");
        }
    }
}
