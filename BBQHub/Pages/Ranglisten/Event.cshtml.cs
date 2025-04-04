using BBQHub.Application.Common.Interfaces;
using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Ranglisten
{
    [Authorize(Roles = "Admin,EventOrganizer")]
    public class EventRanglisteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public EventRanglisteModel(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Event? Event { get; set; }
        public List<Durchgang> Durchgaenge { get; set; } = new();
        public List<Team> Teams { get; set; } = new();

        public Dictionary<int, Dictionary<int, double>> PunkteProDurchgang = new(); // TeamId -> DurchgangId -> Punkte
        public Dictionary<int, double> Gesamtpunkte = new(); // TeamId -> Gesamt

        public Dictionary<int, Dictionary<int, double>> PunkteMitStreichresultat = new();
        public Dictionary<int, double> GesamtMitStreichresultat = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Event = await _context.Events
                .Include(e => e.Durchgaenge)
                    .ThenInclude(d => d.Kriterien)
                .FirstOrDefaultAsync(e => e.Id == Id);

            if (Event == null)
                return NotFound();

            Durchgaenge = Event.Durchgaenge.OrderBy(d => d.Durchgangsnummer).ToList();
            var durchgangIds = Durchgaenge.Select(d => d.Id).ToList();

            var zuweisungen = await _context.EventTeamAssignments
                .Where(x => x.EventId == Event.Id)
                .Include(x => x.Team)
                .ToListAsync();

            Teams = zuweisungen.Select(z => z.Team).Distinct().ToList();

            var bewertungen = await _context.Bewertungen
                .Where(b => durchgangIds.Contains(b.DurchgangId))
                .ToListAsync();

            foreach (var team in Teams)
            {
                PunkteProDurchgang[team.Id] = new Dictionary<int, double>();
                double gesamt = 0;

                foreach (var dg in Durchgaenge)
                {
                    var punkte = bewertungen
                        .Where(b => b.DurchgangId == dg.Id && b.TeamId == team.Id)
                        .Sum(b => b.GewichteteNote);

                    PunkteProDurchgang[team.Id][dg.Id] = punkte;
                    gesamt += punkte;
                }

                Gesamtpunkte[team.Id] = gesamt;
            }

            if (Event.EnableStreichresultate)
            {
                foreach (var team in Teams)
                {
                    PunkteMitStreichresultat[team.Id] = new Dictionary<int, double>();
                    double gesamtMitStreich = 0;

                    foreach (var dg in Durchgaenge)
                    {
                        var kriterien = dg.Kriterien;
                        double summe = 0;

                        foreach (var kriterium in kriterien)
                        {
                            var werte = bewertungen
                                .Where(b => b.DurchgangId == dg.Id && b.TeamId == team.Id && b.KriteriumId == kriterium.Id)
                                .Select(b => b.GewichteteNote)
                                .OrderBy(x => x)
                                .ToList();

                            if (werte.Count > 2)
                                werte = werte.Skip(1).Take(werte.Count - 2).ToList(); // Beste & schlechteste Note entfernen

                            summe += werte.Sum();
                        }

                        PunkteMitStreichresultat[team.Id][dg.Id] = summe;
                        gesamtMitStreich += summe;
                    }

                    GesamtMitStreichresultat[team.Id] = gesamtMitStreich;
                }
            }


            return Page();
        }

        public async Task<IActionResult> OnPostExportGesamtAsync()
        {
            var pdfBytes = await _exportService.ExportRanglisteAsync(Id, null, "gesamt");
            return File(pdfBytes, "application/pdf", $"Rangliste_Gesamt_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        public async Task<IActionResult> OnPostExportDurchgangAsync(int durchgangId)
        {
            var pdfBytes = await _exportService.ExportRanglisteAsync(Id, durchgangId, "durchgang");
            return File(pdfBytes, "application/pdf", $"Rangliste_DG{durchgangId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

    }
}
