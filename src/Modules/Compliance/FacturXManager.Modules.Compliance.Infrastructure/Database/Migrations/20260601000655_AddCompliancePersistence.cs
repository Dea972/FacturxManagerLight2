using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturXManager.Modules.Compliance.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCompliancePersistence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        migrationBuilder.CreateTable(
            name: "credit_notes",
            schema: "compliance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "TEXT", nullable: false),
                number = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                issue_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                profile = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                operation_nature = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                seller_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                seller_legal_registration_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                seller_legal_organization_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                seller_vat_identifier = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                seller_address_line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                seller_address_line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                seller_postal_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                seller_city = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                seller_country_code = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                seller_electronic_address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                seller_electronic_address_scheme = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                buyer_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                buyer_legal_registration_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                buyer_legal_organization_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                buyer_vat_identifier = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                buyer_address_line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                buyer_address_line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                buyer_postal_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                buyer_city = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                buyer_country_code = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                buyer_electronic_address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                buyer_electronic_address_scheme = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                original_invoice_number = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                original_invoice_issue_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                credit_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                buyer_reference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                payment_means_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                status = table.Column<int>(type: "INTEGER", nullable: false),
                channel_deposit_id = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_credit_notes", x => x.id));

        migrationBuilder.CreateTable(
            name: "invoices",
            schema: "compliance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "TEXT", nullable: false),
                number = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                issue_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                profile = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                operation_nature = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                seller_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                seller_legal_registration_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                seller_legal_organization_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                seller_vat_identifier = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                seller_address_line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                seller_address_line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                seller_postal_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                seller_city = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                seller_country_code = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                seller_electronic_address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                seller_electronic_address_scheme = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                buyer_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                buyer_legal_registration_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                buyer_legal_organization_id = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                buyer_vat_identifier = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                buyer_address_line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                buyer_address_line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                buyer_postal_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                buyer_city = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                buyer_country_code = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                buyer_electronic_address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                buyer_electronic_address_scheme = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                payment_means_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                payment_iban = table.Column<string>(type: "TEXT", maxLength: 34, nullable: true),
                payment_due_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                buyer_reference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                purchase_order_reference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                channel_deposit_id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_invoices", x => x.id));

        migrationBuilder.CreateTable(
            name: "credit_note_lines",
            schema: "compliance",
            columns: table => new
            {
                line_id = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                unit_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                unit_net_price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                vat_category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                vat_rate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                credit_note_id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_credit_note_lines", x => x.line_id);
                table.ForeignKey(
                    name: "fk_credit_note_lines_credit_notes_credit_note_id",
                    column: x => x.credit_note_id,
                    principalSchema: "compliance",
                    principalTable: "credit_notes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "credit_note_vat_breakdowns",
            schema: "compliance",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                vat_category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                rate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                basis_amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                tax_amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                credit_note_id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_credit_note_vat_breakdowns", x => x.id);
                table.ForeignKey(
                    name: "fk_credit_note_vat_breakdowns_credit_notes_credit_note_id",
                    column: x => x.credit_note_id,
                    principalSchema: "compliance",
                    principalTable: "credit_notes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "invoice_lines",
            schema: "compliance",
            columns: table => new
            {
                line_id = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                unit_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                unit_net_price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                vat_category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                vat_rate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                invoice_id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_invoice_lines", x => x.line_id);
                table.ForeignKey(
                    name: "fk_invoice_lines_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalSchema: "compliance",
                    principalTable: "invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "invoice_vat_breakdowns",
            schema: "compliance",
            columns: table => new
            {
                id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                vat_category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                rate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                basis_amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                tax_amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                invoice_id = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_invoice_vat_breakdowns", x => x.id);
                table.ForeignKey(
                    name: "fk_invoice_vat_breakdowns_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalSchema: "compliance",
                    principalTable: "invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_credit_note_lines_credit_note_id",
            schema: "compliance",
            table: "credit_note_lines",
            column: "credit_note_id");

        migrationBuilder.CreateIndex(
            name: "ix_credit_note_vat_breakdowns_credit_note_id",
            schema: "compliance",
            table: "credit_note_vat_breakdowns",
            column: "credit_note_id");

        migrationBuilder.CreateIndex(
            name: "ix_credit_notes_number",
            schema: "compliance",
            table: "credit_notes",
            column: "number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_invoice_lines_invoice_id",
            schema: "compliance",
            table: "invoice_lines",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "ix_invoice_vat_breakdowns_invoice_id",
            schema: "compliance",
            table: "invoice_vat_breakdowns",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "ix_invoices_number",
            schema: "compliance",
            table: "invoices",
            column: "number",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "credit_note_lines",
            schema: "compliance");

        migrationBuilder.DropTable(
            name: "credit_note_vat_breakdowns",
            schema: "compliance");

        migrationBuilder.DropTable(
            name: "invoice_lines",
            schema: "compliance");

        migrationBuilder.DropTable(
            name: "invoice_vat_breakdowns",
            schema: "compliance");

        migrationBuilder.DropTable(
            name: "credit_notes",
            schema: "compliance");

        migrationBuilder.DropTable(
            name: "invoices",
            schema: "compliance");
    }
}
