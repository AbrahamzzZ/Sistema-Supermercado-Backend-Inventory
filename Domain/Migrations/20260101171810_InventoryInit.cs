using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class InventoryInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MOVIMIENTO_STOCK",
                columns: table => new
                {
                    ID_MOVIMIENTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: false),
                    TIPO = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CANTIDAD = table.Column<int>(type: "int", nullable: false),
                    REFERENCIA = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FECHA = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOVIMIENTO_STOCK", x => x.ID_MOVIMIENTO);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCTO_STOCK",
                columns: table => new
                {
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: false),
                    STOCK = table.Column<int>(type: "int", nullable: false),
                    PRECIO_COMPRA = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    PRECIO_VENTA = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTO_STOCK", x => x.ID_PRODUCTO);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MOVIMIENTO_STOCK");

            migrationBuilder.DropTable(
                name: "PRODUCTO_STOCK");
        }
    }
}
