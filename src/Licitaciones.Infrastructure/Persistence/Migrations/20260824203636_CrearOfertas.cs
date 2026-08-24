using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licitaciones.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearOfertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ofertas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licitacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto_crc = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ofertas", x => x.id);
                    table.CheckConstraint("ck_ofertas_monto_positivo", "monto_crc > 0");
                    table.ForeignKey(
                        name: "FK_ofertas_licitaciones_licitacion_id",
                        column: x => x.licitacion_id,
                        principalTable: "licitaciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ofertas_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ofertas_proveedor_id",
                table: "ofertas",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ux_ofertas_licitacion_proveedor",
                table: "ofertas",
                columns: new[] { "licitacion_id", "proveedor_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ofertas");
        }
    }
}
