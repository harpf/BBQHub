using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTokenToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueToken",
                table: "EventTeamAssignments");

            migrationBuilder.AddColumn<int>(
                name: "Token",
                table: "EventTeamAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "EventTeamAssignments");

            migrationBuilder.AddColumn<string>(
                name: "UniqueToken",
                table: "EventTeamAssignments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
