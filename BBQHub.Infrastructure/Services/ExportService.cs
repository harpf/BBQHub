using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static QuestPDF.Helpers.Colors;

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

        var isSpontanEvent = eventData.Typ == EventType.SpontanTeilnahme;
        var durchgaenge = eventData.Durchgaenge.OrderBy(d => d.Durchgangsnummer).ToList();

        var logos = await _context.EventLogos
            .Where(l => l.EventId == eventId && l.IsActive)
            .ToListAsync();
        var veranstalterLogos = logos.Where(l => l.Type == LogoType.Veranstalter).ToList();
        var hauptsponsoren = logos.Where(l => l.Type == LogoType.Hauptsponsor).ToList();
        var nebensponsoren = logos.Where(l => l.Type == LogoType.Nebensponsor).ToList();

        var bewertungen = await _context.Bewertungen
            .Where(b => b.Durchgang.EventId == eventId)
            .ToListAsync();

        List<string> namen;
        Dictionary<string, double> punkteProName;
        Dictionary<string, Dictionary<int, double>> durchgangsPunkte;
        List<SpontanTeilnahme>? teilnahmen = null;

        if (isSpontanEvent)
        {
            teilnahmen = await _context.spontanTeilnahmen
                .Where(t => t.Durchgang.EventId == eventId)
                .Include(t => t.Durchgang)
                .ToListAsync();

            namen = teilnahmen
                .Select(t => $"{t.Vorname} {t.Nachname}")
                .Distinct()
                .ToList();

            punkteProName = new();
            durchgangsPunkte = new();

            foreach (var name in namen)
            {
                punkteProName[name] = 0;
                durchgangsPunkte[name] = new();

                foreach (var dg in durchgaenge)
                {
                    var punkte = bewertungen
                        .Where(b => b.DurchgangId == dg.Id && b.SpontanTeilnahme != null &&
                                    $"{b.SpontanTeilnahme.Vorname} {b.SpontanTeilnahme.Nachname}" == name)
                        .Sum(b => b.GewichteteNote);

                    durchgangsPunkte[name][dg.Id] = punkte;
                    punkteProName[name] += punkte;
                }
            }
        }
        else
        {
            var teamZuweisungen = await _context.EventTeamAssignments
                .Where(x => x.EventId == eventId)
                .Include(x => x.Team)
                .ToListAsync();

            var teams = teamZuweisungen.Select(x => x.Team).ToList();

            namen = teams.Select(t => t.Name).ToList();
            punkteProName = new();
            durchgangsPunkte = new();

            foreach (var team in teams)
            {
                punkteProName[team.Name] = 0;
                durchgangsPunkte[team.Name] = new();

                foreach (var dg in durchgaenge)
                {
                    var punkte = bewertungen
                        .Where(b => b.DurchgangId == dg.Id && b.TeamId == team.Id)
                        .Sum(b => b.GewichteteNote);

                    durchgangsPunkte[team.Name][dg.Id] = punkte;
                    punkteProName[team.Name] += punkte;
                }
            }
        }

        // Falls NUR ein einzelner Durchgang exportiert werden soll:
        if (type == "durchgang" && durchgangId.HasValue)
        {
            var dg = durchgaenge.FirstOrDefault(d => d.Id == durchgangId.Value);
            if (dg == null)
                throw new Exception("Durchgang nicht gefunden");

            // Daten auf nur diesen Durchgang beschränken
            punkteProName = punkteProName
                .ToDictionary(kvp => kvp.Key, kvp => durchgangsPunkte[kvp.Key].GetValueOrDefault(dg.Id));

            durchgaenge = durchgaenge.Where(d => d.Id == dg.Id).ToList();
        }


        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4); //.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Header().Element(header =>
                {
                    if (isSpontanEvent)
                    {
                        header.Column(headerCol =>
                        {
                            // Zeile 1: Eventtitel
                            headerCol.Item().AlignCenter().Text(text =>
                            {
                                text.Span(eventData.Name).FontSize(28).Bold();
                                if (!string.IsNullOrEmpty(eventData.City))
                                {
                                    text.Line($"{eventData.City}, {eventData.StartDate:yyyy}").FontSize(20).Bold();
                                }
                            });

                            // Zeile 2: Kriterien-Tabelle & Logos
                            headerCol.Item().PaddingTop(10).Row(row =>
                            {
                                // Kompakte Tabelle links – nur so breit wie nötig
                                row.ConstantItem(220).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(60);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Kriterium").Bold().FontSize(11);
                                        header.Cell().AlignRight().Text("Gewichtung").Bold().FontSize(11);
                                    });

                                    foreach (var kriterium in durchgaenge.First().Kriterien.OrderBy(k => k.Name))
                                    {
                                        table.Cell().Text(kriterium.Name).FontSize(10);
                                        table.Cell().AlignRight().Text($"{kriterium.Gewichtung:0.00}").FontSize(10);
                                    }
                                });

                                // Platz zwischen Tabelle und Logos
                                row.ConstantItem(20);

                                // Logos nebeneinander (max. 5 Logos)
                                row.RelativeItem().AlignRight().Row(logoRow =>
                                {
                                    var allLogos = veranstalterLogos
                                        .Concat(hauptsponsoren)
                                        .Concat(nebensponsoren)
                                        .Take(5)
                                        .ToList();

                                    foreach (var logo in allLogos)
                                    {
                                        var path = Path.Combine(_webRootPath, logo.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                                        if (File.Exists(path))
                                        {
                                            var image = File.ReadAllBytes(path);
                                            logoRow.ConstantItem(80).Height(60).Image(image).FitArea();
                                        }
                                    }
                                });
                            });
                        });
                    }
                    else
                    {
                        header.Row(row =>
                        {
                            // Veranstalterlogo links (max 100x80)
                            row.RelativeItem().MinWidth(80).Element(container =>
                            {
                                var veranstalter = veranstalterLogos.FirstOrDefault();
                                if (veranstalter != null)
                                {
                                    var path = Path.Combine(_webRootPath, veranstalter.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                                    if (File.Exists(path))
                                    {
                                        var image = File.ReadAllBytes(path);
                                        container.AlignLeft().Height(80).Image(image).FitArea();
                                    }
                                }
                            });

                            // Titel zentriert
                            row.RelativeItem(2).Column(col =>
                            {
                                col.Item().AlignCenter().Text(text =>
                                {
                                    text.Span("Rangliste").FontSize(24).Bold();
                                    text.Line("");
                                    text.Span($"– {eventData.Name}").FontSize(20).Bold();
                                    text.Line("");
                                    text.Span($"{DateTime.Now.Year}").FontSize(14).Italic().FontColor(Colors.Grey.Darken2);
                                });
                            });

                            // Hauptsponsorlogo rechts (max 100x80)
                            row.RelativeItem().MinWidth(80).Element(container =>
                            {
                                var hauptsponsor = hauptsponsoren.FirstOrDefault();
                                if (hauptsponsor != null)
                                {
                                    var path = Path.Combine(_webRootPath, hauptsponsor.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                                    if (File.Exists(path))
                                    {
                                        var image = File.ReadAllBytes(path);
                                        container.AlignRight().Height(80).Image(image).FitArea();
                                    }
                                }
                            });
                        });
                    }
                });

                page.Content().Element(c =>
                {
                    if (isSpontanEvent && type == "durchgang" && durchgangId.HasValue)
                    {
                        var dg = durchgaenge.First(d => d.Id == durchgangId.Value);
                        var kriterien = dg.Kriterien.OrderBy(k => k.Name).ToList();
                        var sauceKriterium = kriterien.FirstOrDefault(k => k.Name.ToLower().Contains("sauce"));

                        var bewertungenInDurchgang = bewertungen
                            .Where(b => b.DurchgangId == dg.Id && b.SpontanTeilnahmeId.HasValue)
                            .ToList();

                        var teilnehmernamen = teilnahmen
                            .Select(t => new { Name = $"{t.Vorname} {t.Nachname}", TeilnahmeId = t.Id })
                            .ToList();

                        var maxSauce = 0.0;
                        var maxGewichtet = 0.0;
                        var resultRows = new List<(string Name, Dictionary<double, double> Punkte, double Total, double Gewichtet)>();

                        foreach (var teilnehmer in teilnehmernamen)
                        {
                            var bewertungenTeilnehmer = bewertungenInDurchgang
                                .Where(b => b.SpontanTeilnahmeId == teilnehmer.TeilnahmeId)
                                .ToList();

                            var punkteProKriterium = new Dictionary<double, double>();
                            double total = 0;
                            double gewichtet = 0;

                            foreach (var kriterium in kriterien)
                            {
                                var bewertung = bewertungenTeilnehmer.FirstOrDefault(b => b.KriteriumId == kriterium.Id);
                                var punkte = bewertung?.Punkte ?? 0;
                                punkteProKriterium[kriterium.Id] = punkte;
                                total += punkte;
                                gewichtet += punkte * kriterium.Gewichtung;
                            }

                            if (sauceKriterium != null && punkteProKriterium.TryGetValue(sauceKriterium.Id, out var sauce))
                                maxSauce = Math.Max(maxSauce, sauce);

                            maxGewichtet = Math.Max(maxGewichtet, gewichtet);
                            resultRows.Add((teilnehmer.Name, punkteProKriterium, total, gewichtet));
                        }

                        // Tabelle
                        c.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40); // Platz
                                columns.RelativeColumn();   // Name
                                foreach (var _ in kriterien)
                                    columns.ConstantColumn(50); // Punkte je Kriterium
                                columns.ConstantColumn(60); // Total
                                columns.ConstantColumn(80); // Gewichtet
                            });

                            // Kopfzeile
                            table.Header(header =>
                            {
                                header.Cell().Text("#").Bold();
                                header.Cell().Text("Teilnehmer").Bold();
                                foreach (var kriterium in kriterien)
                                    header.Cell().Text(kriterium.Name).Bold();
                                header.Cell().Text("Total").Bold();
                                header.Cell().Text(eventData.EnableStreichresultate ? "Strichbenotung" : "Gewichtet").Bold();
                            });

                            // Inhalte
                            int platz = 1;
                            foreach (var row in resultRows.OrderByDescending(r => r.Gewichtet))
                            {
                                table.Cell().Text($"{platz++}");
                                table.Cell().Text(row.Name);

                                foreach (var kriterium in kriterien)
                                {
                                    var punkt = row.Punkte[kriterium.Id];

                                    table.Cell().Element(container =>
                                    {
                                        container
                                            .Background((sauceKriterium != null && kriterium.Id == sauceKriterium.Id && punkt == maxSauce)
                                                ? Colors.Green.Lighten3
                                                : Colors.White)
                                            .Text(punkt.ToString());
                                    });

                                }

                                table.Cell().Text(row.Total.ToString());

                                table.Cell().Element(cell =>
                                {
                                    if (Math.Abs(row.Gewichtet - maxGewichtet) < 0.01)
                                        cell = cell.Background(Colors.Yellow.Lighten3);

                                    cell.Text($"{row.Gewichtet:0.00}");
                                });
                            }
                        });
                    }
                    else
                    {
                        c.Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(40);
                                cols.RelativeColumn();
                                foreach (var _ in durchgaenge)
                                    cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#").Bold();
                                header.Cell().Text(isSpontanEvent ? "Teilnehmer" : "Team").Bold();
                                foreach (var dg in durchgaenge)
                                    header.Cell().Text($"{dg.Durchgangsnummer} {dg.Name}").Bold();
                                header.Cell().Text("Gesamt").Bold();
                            });

                            int platz = 1;
                            foreach (var (name, total) in punkteProName.OrderByDescending(kv => kv.Value))
                            {
                                table.Cell().Text($"{platz++}");
                                table.Cell().Text(name);
                                foreach (var dg in durchgaenge)
                                    table.Cell().Text($"{durchgangsPunkte[name][dg.Id]:0.0}");
                                table.Cell().Text($"{total:0.0}");
                            }
                        });
                    }
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
                                    logoRow.ConstantItem(60).Height(40).Image(File.ReadAllBytes(path)).FitHeight();
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