using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelSistemi.Migrations
{
    /// <inheritdoc />
    public partial class KonteynerModelleriEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BaslamaTarihi",
                table: "Insaatlar",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TamamlanmaYuzdesi",
                table: "Insaatlar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Konteynerler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Enlem = table.Column<double>(type: "double precision", nullable: false),
                    Boylam = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konteynerler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KonteynerAPersoneller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KonteynerId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonteynerAPersoneller", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KonteynerAPersoneller_Konteynerler_KonteynerId",
                        column: x => x.KonteynerId,
                        principalTable: "Konteynerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KonteynerAPersoneller_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "objectid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KonteynerBPersoneller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KonteynerId = table.Column<int>(type: "integer", nullable: false),
                    BPersonelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonteynerBPersoneller", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KonteynerBPersoneller_Konteynerler_KonteynerId",
                        column: x => x.KonteynerId,
                        principalTable: "Konteynerler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KonteynerBPersoneller_b_personelleri_BPersonelId",
                        column: x => x.BPersonelId,
                        principalTable: "b_personelleri",
                        principalColumn: "objectid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KonteynerAPersoneller_KonteynerId",
                table: "KonteynerAPersoneller",
                column: "KonteynerId");

            migrationBuilder.CreateIndex(
                name: "IX_KonteynerAPersoneller_PersonelId",
                table: "KonteynerAPersoneller",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_KonteynerBPersoneller_BPersonelId",
                table: "KonteynerBPersoneller",
                column: "BPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_KonteynerBPersoneller_KonteynerId",
                table: "KonteynerBPersoneller",
                column: "KonteynerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KonteynerAPersoneller");

            migrationBuilder.DropTable(
                name: "KonteynerBPersoneller");

            migrationBuilder.DropTable(
                name: "Konteynerler");

            migrationBuilder.DropColumn(
                name: "BaslamaTarihi",
                table: "Insaatlar");

            migrationBuilder.DropColumn(
                name: "TamamlanmaYuzdesi",
                table: "Insaatlar");
        }
    }
}
