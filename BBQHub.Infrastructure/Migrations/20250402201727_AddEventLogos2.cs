using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLogos2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogo_Events_EventId",
                table: "EventLogo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventLogo",
                table: "EventLogo");

            migrationBuilder.RenameTable(
                name: "EventLogo",
                newName: "EventLogos");

            migrationBuilder.RenameIndex(
                name: "IX_EventLogo_EventId",
                table: "EventLogos",
                newName: "IX_EventLogos_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventLogos",
                table: "EventLogos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogos_Events_EventId",
                table: "EventLogos",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogos_Events_EventId",
                table: "EventLogos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventLogos",
                table: "EventLogos");

            migrationBuilder.RenameTable(
                name: "EventLogos",
                newName: "EventLogo");

            migrationBuilder.RenameIndex(
                name: "IX_EventLogos_EventId",
                table: "EventLogo",
                newName: "IX_EventLogo_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventLogo",
                table: "EventLogo",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogo_Events_EventId",
                table: "EventLogo",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
