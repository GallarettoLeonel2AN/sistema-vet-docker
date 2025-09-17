using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class VetSinTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Personas_VeterinarioId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_VeterinarioId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "VeterinarioId",
                table: "Turnos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VeterinarioId",
                table: "Turnos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_VeterinarioId",
                table: "Turnos",
                column: "VeterinarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Personas_VeterinarioId",
                table: "Turnos",
                column: "VeterinarioId",
                principalTable: "Personas",
                principalColumn: "Id");
        }
    }
}
