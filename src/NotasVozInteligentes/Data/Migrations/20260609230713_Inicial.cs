using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotasVozInteligentes.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Contenido = table.Column<string>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotasVoz",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RutaAudio = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    DuracionSegundos = table.Column<double>(type: "REAL", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasVoz", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vocabulario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Termino = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vocabulario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vocabulario_Termino",
                table: "Vocabulario",
                column: "Termino",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "NotasVoz");

            migrationBuilder.DropTable(
                name: "Vocabulario");
        }
    }
}
