using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Infrastructure.Data;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _context;

    public ExportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportRanglisteAsync(int eventId, int? durchgangId, string type)
    {
        var eventData = await _context.Events
            .Include(e => e.Durchgaenge)
                .ThenInclude(d => d.Kriterien)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventData == null)
            throw new Exception("Event nicht gefunden");

        var durchgaenge = eventData.Durchgaenge.OrderBy(d => d.Durchgangsnummer).ToList();

        var teamZuweisungen = await _context.EventTeamAssignments
            .Where(x => x.EventId == eventId)
            .Include(x => x.Team)
            .ToListAsync();

        var teams = teamZuweisungen.Select(x => x.Team).ToList();

        var bewertungen = await _context.Bewertungen
            .Where(b => b.Durchgang.EventId == eventId)
            .ToListAsync();

        return QuestPDF.Fluent.Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Row(row =>
                {
                    row.RelativeColumn().Text($"Rangliste – {eventData.Name}").FontSize(20).Bold();
                    row.ConstantColumn(100).Image(GetSponsorLogoBytes());
                });

                page.Content().Element(container =>
                {
                    container.Table(table =>
                    {
                        if (type == "durchgang" && durchgangId.HasValue)
                        {
                            var dg = durchgaenge.FirstOrDefault(d => d.Id == durchgangId.Value);
                            if (dg == null)
                                throw new Exception("Durchgang nicht gefunden");

                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#").Bold();
                                header.Cell().Text("Team").Bold();
                                header.Cell().Text($"Punkte für DG {dg.Durchgangsnummer}").Bold();
                            });

                            var punkteMap = teams.ToDictionary(
                                t => t.Id,
                                t => bewertungen.Where(b => b.DurchgangId == dg.Id && b.TeamId == t.Id).Sum(b => b.GewichteteNote)
                            );

                            var sorted = punkteMap.OrderByDescending(kv => kv.Value).ToList();
                            int platz = 1;
                            foreach (var (teamId, punkte) in sorted)
                            {
                                var team = teams.First(t => t.Id == teamId);
                                table.Cell().Text($"{platz++}");
                                table.Cell().Text(team.Name);
                                table.Cell().Text($"{punkte:0.0}");
                            }
                        }
                        else // Gesamtrangliste
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                foreach (var _ in durchgaenge)
                                    columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#").Bold();
                                header.Cell().Text("Team").Bold();
                                foreach (var dg in durchgaenge)
                                    header.Cell().Text($"DG {dg.Durchgangsnummer}").Bold();
                                header.Cell().Text("Gesamt").Bold();
                            });

                            var punkteProTeam = new Dictionary<int, double>();
                            var durchgangsPunkte = new Dictionary<int, Dictionary<int, double>>();

                            foreach (var team in teams)
                            {
                                punkteProTeam[team.Id] = 0;
                                durchgangsPunkte[team.Id] = new();

                                foreach (var dg in durchgaenge)
                                {
                                    var punkt = bewertungen
                                        .Where(b => b.DurchgangId == dg.Id && b.TeamId == team.Id)
                                        .Sum(b => b.GewichteteNote);
                                    durchgangsPunkte[team.Id][dg.Id] = punkt;
                                    punkteProTeam[team.Id] += punkt;
                                }
                            }

                            var sorted = punkteProTeam.OrderByDescending(kv => kv.Value).ToList();
                            int platz = 1;
                            foreach (var (teamId, gesamt) in sorted)
                            {
                                var team = teams.First(t => t.Id == teamId);
                                table.Cell().Text($"{platz++}");
                                table.Cell().Text(team.Name);
                                foreach (var dg in durchgaenge)
                                    table.Cell().Text($"{durchgangsPunkte[teamId][dg.Id]:0.0}");
                                table.Cell().Text($"{gesamt:0.0}");
                            }
                        }
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated by BBQHub – ");
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy")).SemiBold();
                    });
            });
        }).GeneratePdf();
    }


    private byte[] GetSponsorLogoBytes()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "sponsor.png");
        return File.Exists(path)
            ? File.ReadAllBytes(path)
            : Placeholders.Image(100, 100);
    }
}
