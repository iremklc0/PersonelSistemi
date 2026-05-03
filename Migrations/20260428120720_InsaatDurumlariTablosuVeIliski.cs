using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonelSistemi.Migrations
{
    /// <inheritdoc />
    public partial class InsaatDurumlariTablosuVeIliski : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Durum",
                table: "Insaatlar",
                newName: "DurumId");

            migrationBuilder.AlterColumn<string>(
                name: "InsaatTuru",
                table: "Insaatlar",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "Insaatlar",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "InsaatDurumlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    durum_adi = table.Column<string>(type: "text", nullable: true),
                    renk_kodu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsaatDurumlari", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Insaatlar_DurumId",
                table: "Insaatlar",
                column: "DurumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Insaatlar_InsaatDurumlari_DurumId",
                table: "Insaatlar",
                column: "DurumId",
                principalTable: "InsaatDurumlari",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Insaatlar_InsaatDurumlari_DurumId",
                table: "Insaatlar");

            migrationBuilder.DropTable(
                name: "InsaatDurumlari");

            migrationBuilder.DropIndex(
                name: "IX_Insaatlar_DurumId",
                table: "Insaatlar");

            migrationBuilder.RenameColumn(
                name: "DurumId",
                table: "Insaatlar",
                newName: "Durum");

            migrationBuilder.AlterColumn<string>(
                name: "InsaatTuru",
                table: "Insaatlar",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "Insaatlar",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
