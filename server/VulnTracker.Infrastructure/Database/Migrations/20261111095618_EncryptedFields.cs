using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VulnTracker.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class EncryptedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReporterEmail",
                table: "findings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReporterEmail",
                table: "findings",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
