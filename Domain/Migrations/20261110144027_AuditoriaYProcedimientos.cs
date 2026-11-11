using System;
using Domain.Migrations.Scripts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYProcedimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MOVIMIENTO_STOCK.FECHA -> FECHA_CREACION (se RENOMBRA para no perder datos; EF lo había generado como DROP + ADD)
            migrationBuilder.Sql("UPDATE MOVIMIENTO_STOCK SET FECHA = SYSUTCDATETIME() WHERE FECHA IS NULL;");

            migrationBuilder.RenameColumn(
                name: "FECHA",
                table: "MOVIMIENTO_STOCK",
                newName: "FECHA_CREACION");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "MOVIMIENTO_STOCK",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<int>(
                name: "STOCK",
                table: "PRODUCTO_STOCK",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "PRECIO_VENTA",
                table: "PRODUCTO_STOCK",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PRECIO_COMPRA",
                table: "PRODUCTO_STOCK",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "PRODUCTO_STOCK",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_MODIFICACION",
                table: "PRODUCTO_STOCK",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "PRODUCTO_STOCK",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_MODIFICACION",
                table: "PRODUCTO_STOCK",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_USUARIO_CREACION",
                table: "MOVIMIENTO_STOCK",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HISTORIAL_PRECIO",
                columns: table => new
                {
                    ID_HISTORIAL_PRECIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: false),
                    PRECIO_COMPRA_ANTERIOR = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PRECIO_COMPRA_NUEVO = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PRECIO_VENTA_ANTERIOR = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PRECIO_VENTA_NUEVO = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    REFERENCIA = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ID_USUARIO_CREACION = table.Column<int>(type: "int", nullable: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HISTORIAL_PRECIO", x => x.ID_HISTORIAL_PRECIO);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MOVIMIENTO_STOCK_PRODUCTO_FECHA",
                table: "MOVIMIENTO_STOCK",
                columns: new[] { "ID_PRODUCTO", "FECHA_CREACION" });

            migrationBuilder.CreateIndex(
                name: "IX_HISTORIAL_PRECIO_PRODUCTO_FECHA",
                table: "HISTORIAL_PRECIO",
                columns: new[] { "ID_PRODUCTO", "FECHA_CREACION" },
                descending: new[] { false, true });

            // Stored procedures (después de crear/alterar las tablas que usan)
            migrationBuilder.EjecutarScript("001_PA_REGISTRAR_MOVIMIENTO_STOCK.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_REGISTRAR_MOVIMIENTO_STOCK;");

            migrationBuilder.DropTable(
                name: "HISTORIAL_PRECIO");

            migrationBuilder.DropIndex(
                name: "IX_MOVIMIENTO_STOCK_PRODUCTO_FECHA",
                table: "MOVIMIENTO_STOCK");

            migrationBuilder.DropColumn(
                name: "FECHA_CREACION",
                table: "PRODUCTO_STOCK");

            migrationBuilder.DropColumn(
                name: "FECHA_MODIFICACION",
                table: "PRODUCTO_STOCK");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "PRODUCTO_STOCK");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_MODIFICACION",
                table: "PRODUCTO_STOCK");

            migrationBuilder.DropColumn(
                name: "ID_USUARIO_CREACION",
                table: "MOVIMIENTO_STOCK");

            migrationBuilder.AlterColumn<int>(
                name: "STOCK",
                table: "PRODUCTO_STOCK",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "PRECIO_VENTA",
                table: "PRODUCTO_STOCK",
                type: "decimal(10,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PRECIO_COMPRA",
                table: "PRODUCTO_STOCK",
                type: "decimal(10,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_CREACION",
                table: "MOVIMIENTO_STOCK",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.RenameColumn(
                name: "FECHA_CREACION",
                table: "MOVIMIENTO_STOCK",
                newName: "FECHA");
        }
    }
}
