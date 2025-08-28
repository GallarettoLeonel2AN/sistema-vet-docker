using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class CambiosTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Disponibilidades");

            migrationBuilder.CreateTable(
                name: "ConfiguracionVeterinarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    DuracionMinutosPorConsulta = table.Column<int>(type: "int", nullable: false),
                    TrabajaLunes = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaMartes = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaMiercoles = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaJueves = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaViernes = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaSabado = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaDomingo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionVeterinarias", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionVeterinarias");

            migrationBuilder.CreateTable(
                name: "Disponibilidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VeterinarioId = table.Column<int>(type: "int", nullable: false),
                    DuracionMinutosPorConsulta = table.Column<int>(type: "int", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    TrabajaDomingo = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaJueves = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaLunes = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaMartes = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaMiercoles = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaSabado = table.Column<bool>(type: "bit", nullable: false),
                    TrabajaViernes = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disponibilidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disponibilidades_Personas_VeterinarioId",
                        column: x => x.VeterinarioId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilidades_VeterinarioId",
                table: "Disponibilidades",
                column: "VeterinarioId");
        }
    }
}
