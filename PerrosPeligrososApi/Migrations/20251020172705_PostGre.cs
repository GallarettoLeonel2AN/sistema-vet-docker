using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PerrosPeligrososApi.Migrations
{
    /// <inheritdoc />
    public partial class PostGre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerrosPeligrosos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Raza = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MascotaIdOriginal = table.Column<int>(type: "integer", nullable: false),
                    ClienteDni = table.Column<long>(type: "bigint", nullable: false),
                    ClienteNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClienteApellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaRegistroApi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerrosPeligrosos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChipsPerroPeligroso",
                columns: table => new
                {
                    PerroPeligrosoId = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChipsPerroPeligroso", x => x.PerroPeligrosoId);
                    table.ForeignKey(
                        name: "FK_ChipsPerroPeligroso_PerrosPeligrosos_PerroPeligrosoId",
                        column: x => x.PerroPeligrosoId,
                        principalTable: "PerrosPeligrosos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChipsPerroPeligroso");

            migrationBuilder.DropTable(
                name: "PerrosPeligrosos");
        }
    }
}
