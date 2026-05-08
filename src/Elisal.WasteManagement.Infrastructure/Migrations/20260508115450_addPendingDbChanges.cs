using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elisal.WasteManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPendingDbChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSkipped",
                table: "RoutePointExecutionStatuses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SkipReason",
                table: "RoutePointExecutionStatuses",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AssignedDriverId",
                table: "Rota",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rota_AssignedDriverId",
                table: "Rota",
                column: "AssignedDriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rota_Usuario_AssignedDriverId",
                table: "Rota",
                column: "AssignedDriverId",
                principalTable: "Usuario",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rota_Usuario_AssignedDriverId",
                table: "Rota");

            migrationBuilder.DropIndex(
                name: "IX_Rota_AssignedDriverId",
                table: "Rota");

            migrationBuilder.DropColumn(
                name: "IsSkipped",
                table: "RoutePointExecutionStatuses");

            migrationBuilder.DropColumn(
                name: "SkipReason",
                table: "RoutePointExecutionStatuses");

            migrationBuilder.DropColumn(
                name: "AssignedDriverId",
                table: "Rota");
        }
    }
}
