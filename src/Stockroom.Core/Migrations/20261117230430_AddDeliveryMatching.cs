using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockroom.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DeliveryNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TotalCents = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AutoThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    SuggestThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumLead = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysAfterOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeliveryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Decision = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplierPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    DatePoints = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferencePoints = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryMatches_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryMatches_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MatchingSettings",
                columns: new[] { "Id", "AutoThreshold", "DaysAfterOrder", "MinimumLead", "SuggestThreshold" },
                values: new object[] { 1, 80, 30, 10, 50 });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_SupplierId",
                table: "Deliveries",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMatches_DeliveryId",
                table: "DeliveryMatches",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMatches_PurchaseOrderId",
                table: "DeliveryMatches",
                column: "PurchaseOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryMatches");

            migrationBuilder.DropTable(
                name: "MatchingSettings");

            migrationBuilder.DropTable(
                name: "Deliveries");
        }
    }
}
