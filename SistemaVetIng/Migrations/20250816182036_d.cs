using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dia",
                table: "Disponibilidades");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HoraInicio",
                table: "Disponibilidades",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HoraFin",
                table: "Disponibilidades",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "DuracionMinutosPorConsulta",
                table: "Disponibilidades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaDomingo",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaJueves",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaLunes",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaMartes",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaMiercoles",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaSabado",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrabajaViernes",
                table: "Disponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuracionMinutosPorConsulta",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaDomingo",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaJueves",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaLunes",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaMartes",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaMiercoles",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaSabado",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "TrabajaViernes",
                table: "Disponibilidades");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HoraInicio",
                table: "Disponibilidades",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HoraFin",
                table: "Disponibilidades",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<string>(
                name: "Dia",
                table: "Disponibilidades",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
