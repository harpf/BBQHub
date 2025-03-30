using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamToBewertung : Migration
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_Bewertungen_Teams_TeamId",
                table: "Bewertungen",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }
    }
}
