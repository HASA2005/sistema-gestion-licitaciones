using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licitaciones.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearTiposCambio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipos_cambio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    crc_por_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipos_cambio", x => x.id);
                    table.CheckConstraint("ck_tipos_cambio_crc_positivo", "crc_por_usd > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ux_tipos_cambio_activo",
                table: "tipos_cambio",
                column: "activo",
                unique: true,
                filter: "activo = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipos_cambio");
        }
    }
}
