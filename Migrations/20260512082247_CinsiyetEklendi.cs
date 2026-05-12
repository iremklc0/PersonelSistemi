using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelSistemi.Migrations
{
    /// <inheritdoc />
    public partial class CinsiyetEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cinsiyet",
                table: "Personeller",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cinsiyet",
                table: "b_personelleri",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cinsiyet",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "cinsiyet",
                table: "b_personelleri");
        }
    }
}
