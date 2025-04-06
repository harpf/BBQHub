using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDatenschutzFieldsToSpontanTeilnahme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "spontanTeilnahmen");

            migrationBuilder.AddColumn<DateTime>(
                name: "DatenschutzDatum",
                table: "spontanTeilnahmen",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatenschutzUnterschriftBild",
                table: "spontanTeilnahmen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nachname",
                table: "spontanTeilnahmen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeilnahmeUnterschriftBild",
                table: "spontanTeilnahmen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeilnahmeUnterschriftDatum",
                table: "spontanTeilnahmen",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vorname",
                table: "spontanTeilnahmen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Menue",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatenschutzDatum",
                table: "spontanTeilnahmen");

            migrationBuilder.DropColumn(
                name: "DatenschutzUnterschriftBild",
                table: "spontanTeilnahmen");

            migrationBuilder.DropColumn(
                name: "Nachname",
                table: "spontanTeilnahmen");

            migrationBuilder.DropColumn(
                name: "TeilnahmeUnterschriftBild",
                table: "spontanTeilnahmen");

            migrationBuilder.DropColumn(
                name: "TeilnahmeUnterschriftDatum",
                table: "spontanTeilnahmen");

            migrationBuilder.DropColumn(
                name: "Vorname",
                table: "spontanTeilnahmen");

            migrationBuilder.DropColumn(
                name: "Menue",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "spontanTeilnahmen",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
