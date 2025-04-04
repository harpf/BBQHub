using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpontanTeilnahmeToBewertung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bewertungen_Teams_TeamId",
                table: "Bewertungen");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Bewertungen",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SpontanTeilnahmeId",
                table: "Bewertungen",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bewertungen_SpontanTeilnahmeId",
                table: "Bewertungen",
                column: "SpontanTeilnahmeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bewertungen_Teams_TeamId",
                table: "Bewertungen",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bewertungen_spontanTeilnahmen_SpontanTeilnahmeId",
                table: "Bewertungen",
                column: "SpontanTeilnahmeId",
                principalTable: "spontanTeilnahmen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bewertungen_Teams_TeamId",
                table: "Bewertungen");

            migrationBuilder.DropForeignKey(
                name: "FK_Bewertungen_spontanTeilnahmen_SpontanTeilnahmeId",
                table: "Bewertungen");

            migrationBuilder.DropIndex(
                name: "IX_Bewertungen_SpontanTeilnahmeId",
                table: "Bewertungen");

            migrationBuilder.DropColumn(
                name: "SpontanTeilnahmeId",
                table: "Bewertungen");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Bewertungen",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bewertungen_Teams_TeamId",
                table: "Bewertungen",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
