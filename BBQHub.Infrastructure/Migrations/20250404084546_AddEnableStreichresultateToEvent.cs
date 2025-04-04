using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnableStreichresultateToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableStreichresultate",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableStreichresultate",
                table: "Events");
        }
    }
}
