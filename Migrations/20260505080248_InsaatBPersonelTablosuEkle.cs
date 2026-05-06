using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelSistemi.Migrations
{
    /// <inheritdoc />
    public partial class InsaatBPersonelTablosuEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsaatBPersonelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InsaatId = table.Column<int>(type: "integer", nullable: false),
                    BPersonelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsaatBPersonelleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsaatBPersonelleri_Insaatlar_InsaatId",
                        column: x => x.InsaatId,
                        principalTable: "Insaatlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsaatBPersonelleri_b_personelleri_BPersonelId",
                        column: x => x.BPersonelId,
                        principalTable: "b_personelleri",
                        principalColumn: "objectid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsaatBPersonelleri_BPersonelId",
                table: "InsaatBPersonelleri",
                column: "BPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_InsaatBPersonelleri_InsaatId",
                table: "InsaatBPersonelleri",
                column: "InsaatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsaatBPersonelleri");
        }
    }
}
