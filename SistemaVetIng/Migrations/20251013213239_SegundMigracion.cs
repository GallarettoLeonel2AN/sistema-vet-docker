using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class SegundMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraFin",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaDomingo",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaJueves",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaLunes",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaMartes",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaMiercoles",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaSabado",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.DropColumn(
                name: "TrabajaViernes",
                table: "ConfiguracionVeterinarias");

            migrationBuilder.CreateTable(
                name: "HorarioDia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    HoraInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoraFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfiguracionVeterinariaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioDia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorarioDia_ConfiguracionVeterinarias_ConfiguracionVeterinariaId",
                        column: x => x.ConfiguracionVeterinariaId,
                        principalTable: "ConfiguracionVeterinarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorarioDia_ConfiguracionVeterinariaId",
                table: "HorarioDia",
                column: "ConfiguracionVeterinariaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorarioDia");

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraFin",
                table: "ConfiguracionVeterinarias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraInicio",
                table: "ConfiguracionVeterinarias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaDomingo",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaJueves",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaLunes",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaMartes",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaMiercoles",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaSabado",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaViernes",
                table: "ConfiguracionVeterinarias",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
