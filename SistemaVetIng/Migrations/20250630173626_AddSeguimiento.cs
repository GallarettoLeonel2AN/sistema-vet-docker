using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class AddSeguimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAplicacion",
                table: "Vacunas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Dosis",
                table: "Tratamientos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Duracion",
                table: "Tratamientos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frecuencia",
                table: "Tratamientos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Medicamento",
                table: "Tratamientos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "AtencionesVeterinarias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaAplicacion",
                table: "Vacunas");

            migrationBuilder.DropColumn(
                name: "Dosis",
                table: "Tratamientos");

            migrationBuilder.DropColumn(
                name: "Duracion",
                table: "Tratamientos");

            migrationBuilder.DropColumn(
                name: "Frecuencia",
                table: "Tratamientos");

            migrationBuilder.DropColumn(
                name: "Medicamento",
                table: "Tratamientos");

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "AtencionesVeterinarias");
        }
    }
}
