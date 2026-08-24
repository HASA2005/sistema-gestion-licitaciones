using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licitaciones.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearLicitaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estados_licitacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estados_licitacion", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "licitaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    codigo_normalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    presupuesto_estimado_crc = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_cierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licitaciones", x => x.id);
                    table.CheckConstraint("ck_licitaciones_presupuesto_positivo", "presupuesto_estimado_crc > 0");
                    table.ForeignKey(
                        name: "fk_licitaciones_estados_licitacion_estado",
                        column: x => x.estado,
                        principalTable: "estados_licitacion",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "estados_licitacion",
                columns: new[] { "codigo", "nombre" },
                values: new object[,]
                {
                    { "Borrador", "Borrador" },
                    { "Cerrada", "Cerrada" },
                    { "Publicada", "Publicada" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_licitaciones_estado",
                table: "licitaciones",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ux_licitaciones_codigo_normalizado",
                table: "licitaciones",
                column: "codigo_normalizado",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "licitaciones");

            migrationBuilder.DropTable(
                name: "estados_licitacion");
        }
    }
}
