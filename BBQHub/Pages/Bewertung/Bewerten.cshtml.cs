using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Bewertung
{
    public class BewertenModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BewertenModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int JuryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DurchgangId { get; set; }

        [BindProperty]
        public int Token { get; set; }

        public string? ErrorMessage { get; set; }

        public Team? Team { get; set; }

        [BindProperty(SupportsGet = true)]
        public int TeamId {  get; set; }
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public string EventName { get; set; } = "";


        public List<Kriterium> Kriterien { get; set; } = new();

        [BindProperty]
        public Dictionary<int, int> Punkte { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null) return RedirectToPage("/Bewertung/Start");

            // Duplikatsprüfung
            bool exists = await _context.Bewertungen.AnyAsync(b =>
                b.JurorId == juror.Id &&
                b.DurchgangId == DurchgangId &&
                b.TeamId == TeamId);

            if (exists)
            {
                ErrorMessage = "Du hast dieses Team in diesem Durchgang bereits bewertet.";
                EventName = (await _context.Events.FindAsync(EventId))?.Name ?? "Unbekannt";
                Kriterien = await _context.Kriterien
                    .Where(k => k.DurchgangId == DurchgangId)
                    .ToListAsync();

                return Page();
            }

            // Kriterien laden
            var kriterien = await _context.Kriterien
                .Where(k => k.DurchgangId == DurchgangId)
                .ToListAsync();

            // Bewertungen speichern
            foreach (var k in kriterien)
            {
                int punkt = Punkte.ContainsKey(k.Id) ? Punkte[k.Id] : 0;
                double gewichteteNote = punkt * k.Gewichtung;

                _context.Bewertungen.Add(new BBQHub.Domain.Entities.Bewertung
                {
                    JurorId = juror.Id,
                    DurchgangId = DurchgangId,
                    KriteriumId = k.Id,
                    Punkte = punkt,
                    GewichteteNote = gewichteteNote,
                    TeamId = TeamId
                });
            }

            await _context.SaveChangesAsync();

            // Prüfen, ob es noch weitere bewertbare Teams gibt
            var bereitsBewerteteTeamIds = await _context.Bewertungen
                .Where(b => b.JurorId == juror.Id && b.DurchgangId == DurchgangId)
                .Select(b => b.TeamId)
                .Distinct()
                .ToListAsync();

            var weitereTeams = await _context.EventTeamAssignments
                .Where(a => a.EventId == EventId && !bereitsBewerteteTeamIds.Contains(a.TeamId))
                .ToListAsync();

            if (weitereTeams.Any())
            {
                // Zurück zur Teamauswahl
                return RedirectToPage("/Bewertung/TeamCode", new
                {
                    juryId = JuryId,
                    eventId = EventId,
                    durchgangId = DurchgangId
                });
            }

            // Alles bewertet → Danke
            return RedirectToPage("/Bewertung/Danke", new { juryId = JuryId });
        }
    }
}
