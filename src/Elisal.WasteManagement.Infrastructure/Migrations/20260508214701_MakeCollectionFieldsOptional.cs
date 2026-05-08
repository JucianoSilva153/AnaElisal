using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elisal.WasteManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCollectionFieldsOptional : Migration
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

            migrationBuilder.AlterColumn<int>(
                name: "tipo_residuo_id",
                table: "RegistoRecolha",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "quantidade_kg",
                table: "RegistoRecolha",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

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

            migrationBuilder.AlterColumn<int>(
                name: "tipo_residuo_id",
                table: "RegistoRecolha",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "quantidade_kg",
                table: "RegistoRecolha",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);
        }
    }
}
