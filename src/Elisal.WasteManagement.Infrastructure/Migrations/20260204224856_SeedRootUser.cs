using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elisal.WasteManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedRootUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "id", "data_cadastro", "email", "ativo", "nome", "senha_hash", "perfil" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "root@elisal.ao", true, "Root Admin", "$2a$12$kRntuZvjc1inEbVrN//K/uVjXSiPDTuX8eJtmIKa9fc9rlFs483/a", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
