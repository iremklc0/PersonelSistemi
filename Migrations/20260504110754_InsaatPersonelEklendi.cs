using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelSistemi.Migrations
{
    /// <inheritdoc />
    public partial class InsaatPersonelEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsaatPersonelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InsaatId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsaatPersonelleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsaatPersonelleri_Insaatlar_InsaatId",
                        column: x => x.InsaatId,
                        principalTable: "Insaatlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsaatPersonelleri_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "objectid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsaatPersonelleri_InsaatId",
                table: "InsaatPersonelleri",
                column: "InsaatId");

            migrationBuilder.CreateIndex(
                name: "IX_InsaatPersonelleri_PersonelId",
                table: "InsaatPersonelleri",
                column: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsaatPersonelleri");
        }
    }
}
