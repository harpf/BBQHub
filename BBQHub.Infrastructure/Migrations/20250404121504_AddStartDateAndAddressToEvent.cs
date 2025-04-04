using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStartDateAndAddressToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Events",
                newName: "ZipCode");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "Events",
                newName: "Address");
        }
    }
}
