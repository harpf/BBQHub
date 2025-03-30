using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBQHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Durchgaenge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zeitpunkt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Durchgaenge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Durchgaenge_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kriterien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DurchgangId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxWert = table.Column<int>(type: "int", nullable: false),
                    Gewichtung = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kriterien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kriterien_Durchgaenge_DurchgangId",
                        column: x => x.DurchgangId,
                        principalTable: "Durchgaenge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bewertungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JurorId = table.Column<int>(type: "int", nullable: false),
                    DurchgangId = table.Column<int>(type: "int", nullable: false),
                    KriteriumId = table.Column<int>(type: "int", nullable: false),
                    Punkte = table.Column<int>(type: "int", nullable: false),
                    GewichteteNote = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bewertungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bewertungen_Durchgaenge_DurchgangId",
                        column: x => x.DurchgangId,
                        principalTable: "Durchgaenge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bewertungen_Juroren_JurorId",
                        column: x => x.JurorId,
                        principalTable: "Juroren",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bewertungen_Kriterien_KriteriumId",
                        column: x => x.KriteriumId,
                        principalTable: "Kriterien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                });

            migrationBuilder.CreateIndex(
                name: "IX_Bewertungen_DurchgangId",
                table: "Bewertungen",
                column: "DurchgangId");

            migrationBuilder.CreateIndex(
                name: "IX_Bewertungen_JurorId",
                table: "Bewertungen",
                column: "JurorId");

            migrationBuilder.CreateIndex(
                name: "IX_Bewertungen_KriteriumId",
                table: "Bewertungen",
                column: "KriteriumId");

            migrationBuilder.CreateIndex(
                name: "IX_Durchgaenge_EventId",
                table: "Durchgaenge",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Kriterien_DurchgangId",
                table: "Kriterien",
                column: "DurchgangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bewertungen");

            migrationBuilder.DropTable(
                name: "Kriterien");

            migrationBuilder.DropTable(
                name: "Durchgaenge");
        }
    }
}
