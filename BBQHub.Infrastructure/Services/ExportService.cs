
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _context;
    private readonly string _webRootPath;

    public ExportService(ApplicationDbContext context)
    {
        _context = context;
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<byte[]> ExportRanglisteAsync(int eventId, int? durchgangId, string type)
    {
        var eventData = await _context.Events
            .Include(e => e.Durchgaenge).ThenInclude(d => d.Kriterien)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventData == null)
            throw new Exception("Event nicht gefunden");

        var logos = await _context.EventLogos
            .Where(l => l.EventId == eventId && l.IsActive)
            .ToListAsync();

        var veranstalterLogos = logos.Where(l => l.Type == LogoType.Veranstalter).ToList();
        var hauptsponsoren = logos.Where(l => l.Type == LogoType.Hauptsponsor).ToList();
        var nebensponsoren = logos.Where(l => l.Type == LogoType.Nebensponsor).ToList();

        var durchgaenge = eventData.Durchgaenge.OrderBy(d => d.Durchgangsnummer).ToList();

        var teamZuweisungen = await _context.EventTeamAssignments
            .Where(x => x.EventId == eventId)
            .Include(x => x.Team)
            .ToListAsync();

        var teams = teamZuweisungen.Select(x => x.Team).ToList();

        var bewertungen = await _context.Bewertungen
            .Where(b => b.Durchgang.EventId == eventId)
            .ToListAsync();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // Querformat
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(14)); // Größere Schrift allgemein

                page.Header().Row(row =>
                {
                    // LINKS: max. 2 Hauptsponsoren in einer Reihe
                    var maxHauptsponsoren = hauptsponsoren.Take(2).ToList();
                    row.ConstantItem(180).Row(inner =>
                    {
                        foreach (var sponsor in maxHauptsponsoren)
                        {
                            var path = Path.Combine(_webRootPath, sponsor.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (File.Exists(path))
                            {
                                var image = File.ReadAllBytes(path);
                                inner.ConstantItem(80).Element(x =>
                                    x.Image(image).FitArea()
                                );
                            }
                        }
                    });

                    // MITTE: Eventtitel zentriert
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignCenter().Text($"Rangliste – {eventData.Name}")
                            .FontSize(28).Bold().FontColor(Colors.Black);
                        col.Item().AlignCenter().Text($"{DateTime.Now.Year}")
                            .Italic().FontSize(14).FontColor(Colors.Grey.Darken2);
                    });

                    // RECHTS: max. 3 Nebensponsoren in einer Reihe
                    var maxNebensponsoren = nebensponsoren.Take(3).ToList();
                    row.ConstantItem(240).Row(inner =>
                    {
                        foreach (var sponsor in maxNebensponsoren)
                        {
                            var path = Path.Combine(_webRootPath, sponsor.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (File.Exists(path))
                            {
                                var image = File.ReadAllBytes(path);
                                inner.ConstantItem(80).Element(x =>
                                    x.Image(image).FitArea()
                                );
                            }
                        }
                    });
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
                                columns.ConstantColumn(40);
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
                        else
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
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

                page.Footer().Column(footer =>
                {
                    var übrigeLogos = hauptsponsoren.Skip(2).Concat(nebensponsoren.Skip(3)).ToList();
                    if (übrigeLogos.Any())
                    {
                        footer.Item().AlignCenter().Text("Weitere Sponsoren").FontSize(10).SemiBold();
                        footer.Item().Row(logoRow =>
                        {
                            foreach (var sponsor in übrigeLogos)
                            {
                                var path = Path.Combine(_webRootPath, sponsor.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                                if (File.Exists(path))
                                {
                                    var image = File.ReadAllBytes(path);
                                    logoRow.ConstantItem(60).Height(40).Image(image).FitHeight();
                                }
                            }
                        });
                    }

                    footer.Item().AlignCenter().Text(x =>
                    {
                        x.Span("Generated by BBQHub – ");
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy")).SemiBold();
                    });
                });
            });
        }).GeneratePdf();
    }
}
