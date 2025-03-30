using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveZeitpunktFromDurchgang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bewertungen_Kriterien_KriteriumId",
                table: "Bewertungen");

            migrationBuilder.DropForeignKey(
                name: "FK_Durchgaenge_Events_EventId",
                table: "Durchgaenge");

            migrationBuilder.DropIndex(
                name: "IX_Durchgaenge_EventId",
                table: "Durchgaenge");

            migrationBuilder.DropColumn(
                name: "Zeitpunkt",
                table: "Durchgaenge");

            migrationBuilder.AddForeignKey(
                name: "FK_Bewertungen_Kriterien_KriteriumId",
                table: "Bewertungen",
                column: "KriteriumId",
                principalTable: "Kriterien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bewertungen_Kriterien_KriteriumId",
                table: "Bewertungen");

            migrationBuilder.AddColumn<DateTime>(
                name: "Zeitpunkt",
                table: "Durchgaenge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Durchgaenge_EventId",
                table: "Durchgaenge",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bewertungen_Kriterien_KriteriumId",
                table: "Bewertungen",
                column: "KriteriumId",
                principalTable: "Kriterien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Durchgaenge_Events_EventId",
                table: "Durchgaenge",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
