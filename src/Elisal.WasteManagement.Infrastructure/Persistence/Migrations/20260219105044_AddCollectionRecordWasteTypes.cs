using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elisal.WasteManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionRecordWasteTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionRecordWasteTypes",
                columns: table => new
                {
                    CollectionRecordId = table.Column<int>(type: "int", nullable: false),
                    WasteTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionRecordWasteTypes", x => new { x.CollectionRecordId, x.WasteTypeId });
                    table.ForeignKey(
                        name: "FK_CollectionRecordWasteTypes_RegistoRecolha_CollectionRecordId",
                        column: x => x.CollectionRecordId,
                        principalTable: "RegistoRecolha",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionRecordWasteTypes_TipoResiduo_WasteTypeId",
                        column: x => x.WasteTypeId,
                        principalTable: "TipoResiduo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRecordWasteTypes_WasteTypeId",
                table: "CollectionRecordWasteTypes",
                column: "WasteTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionRecordWasteTypes");
        }
    }
}
