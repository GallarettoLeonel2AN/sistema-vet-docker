using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class cambiosssss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AtencionesVeterinarias_TratamientoId",
                table: "AtencionesVeterinarias");

            migrationBuilder.AlterColumn<int>(
                name: "TratamientoId",
                table: "AtencionesVeterinarias",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AtencionesVeterinarias_TratamientoId",
                table: "AtencionesVeterinarias",
                column: "TratamientoId",
                unique: true,
                filter: "[TratamientoId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AtencionesVeterinarias_TratamientoId",
                table: "AtencionesVeterinarias");

            migrationBuilder.AlterColumn<int>(
                name: "TratamientoId",
                table: "AtencionesVeterinarias",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtencionesVeterinarias_TratamientoId",
                table: "AtencionesVeterinarias",
                column: "TratamientoId",
                unique: true);
        }
    }
}
