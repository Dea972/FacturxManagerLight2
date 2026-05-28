using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturXManager.Modules.Transmission.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "transmission");

            migrationBuilder.CreateTable(
                name: "transmissions",
                schema: "transmission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    document_reference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    recipient_siret = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    executing_service = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    channel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    channel_deposit_id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    rejection_reason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    created_on_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_updated_on_utc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transmissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transmission_attempts",
                schema: "transmission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    transmission_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    channel_deposit_id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    raw_response = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transmission_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_transmission_attempts_transmissions_transmission_id",
                        column: x => x.transmission_id,
                        principalSchema: "transmission",
                        principalTable: "transmissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transmission_attempts_transmission_id",
                schema: "transmission",
                table: "transmission_attempts",
                column: "transmission_id");

            migrationBuilder.CreateIndex(
                name: "ix_transmissions_document_reference",
                schema: "transmission",
                table: "transmissions",
                column: "document_reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transmission_attempts",
                schema: "transmission");

            migrationBuilder.DropTable(
                name: "transmissions",
                schema: "transmission");
        }
    }
}
