using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elisal.WasteManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInternalCooperative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cooperativa",
                columns: new[] { "id", "tipos_residuos_aceites", "endereco", "contacto", "email", "nome" },
                values: new object[] { 999, "Todos", "Elisal Sede", "N/A", "interno@elisal.co.ao", "Elisal - Reaproveitamento Interno" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoutePointExecutionStatuses");

            migrationBuilder.DropTable(
                name: "RouteExecutions");

            migrationBuilder.DeleteData(
                table: "Cooperativa",
                keyColumn: "id",
                keyValue: 999);
        }
    }
}
