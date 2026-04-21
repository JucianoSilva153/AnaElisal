using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elisal.WasteManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GestaoResiduos_Fase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SortingCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    CapacityTonsPerDay = table.Column<double>(type: "double", nullable: false),
                    Municipality = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Contact = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SortingCenters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WasteReceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GrossWeightKg = table.Column<double>(type: "double", nullable: false),
                    TareWeightKg = table.Column<double>(type: "double", nullable: false),
                    NetWeightKgStored = table.Column<double>(type: "double", nullable: false),
                    RouteExecutionId = table.Column<int>(type: "int", nullable: true),
                    SortingCenterId = table.Column<int>(type: "int", nullable: false),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSorted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WasteReceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WasteReceptions_RouteExecutions_RouteExecutionId",
                        column: x => x.RouteExecutionId,
                        principalTable: "RouteExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WasteReceptions_SortingCenters_SortingCenterId",
                        column: x => x.SortingCenterId,
                        principalTable: "SortingCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WasteReceptions_Usuario_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SortingBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WasteReceptionId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OperatorUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SortingBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SortingBatches_Usuario_OperatorUserId",
                        column: x => x.OperatorUserId,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SortingBatches_WasteReceptions_WasteReceptionId",
                        column: x => x.WasteReceptionId,
                        principalTable: "WasteReceptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SortingBatchOutputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SortingBatchId = table.Column<int>(type: "int", nullable: false),
                    WasteTypeId = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<double>(type: "double", nullable: false),
                    QualityGrade = table.Column<int>(type: "int", nullable: false),
                    DestinationType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SortingBatchOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SortingBatchOutputs_SortingBatches_SortingBatchId",
                        column: x => x.SortingBatchId,
                        principalTable: "SortingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SortingBatchOutputs_TipoResiduo_WasteTypeId",
                        column: x => x.WasteTypeId,
                        principalTable: "TipoResiduo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_email",
                table: "Usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SortingBatches_OperatorUserId",
                table: "SortingBatches",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SortingBatches_StartDateTime",
                table: "SortingBatches",
                column: "StartDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_SortingBatches_Status",
                table: "SortingBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SortingBatches_WasteReceptionId",
                table: "SortingBatches",
                column: "WasteReceptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SortingBatchOutputs_SortingBatchId",
                table: "SortingBatchOutputs",
                column: "SortingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SortingBatchOutputs_WasteTypeId",
                table: "SortingBatchOutputs",
                column: "WasteTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SortingCenters_Name",
                table: "SortingCenters",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WasteReceptions_DateTime",
                table: "WasteReceptions",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_WasteReceptions_IsSorted",
                table: "WasteReceptions",
                column: "IsSorted");

            migrationBuilder.CreateIndex(
                name: "IX_WasteReceptions_ReceivedByUserId",
                table: "WasteReceptions",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WasteReceptions_RouteExecutionId",
                table: "WasteReceptions",
                column: "RouteExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_WasteReceptions_SortingCenterId",
                table: "WasteReceptions",
                column: "SortingCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SortingBatchOutputs");

            migrationBuilder.DropTable(
                name: "SortingBatches");

            migrationBuilder.DropTable(
                name: "WasteReceptions");

            migrationBuilder.DropTable(
                name: "SortingCenters");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_email",
                table: "Usuario");
        }
    }
}
