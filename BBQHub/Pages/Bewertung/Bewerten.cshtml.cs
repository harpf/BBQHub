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

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TeamId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TeilnehmerId { get; set; }

        public bool IstTeamBewertung => TeamId != null;

        public string EventName { get; set; } = string.Empty;
        public string DurchgangName { get; set; } = string.Empty;

        public string? TeilnehmerCode { get; set; }

        public List<Kriterium> Kriterien { get; set; } = new();

        public Dictionary<int, int> Punkte { get; set; } = new();

        [BindProperty]
        public Dictionary<int, int> BewertetePunkte { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var durchgang = await _context.Durchgaenge
                .Include(d => d.Kriterien)
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Id == DurchgangId);

            if (durchgang == null)
                return NotFound();

            EventName = durchgang.Event?.Name ?? "Unbekanntes Event";
            DurchgangName = durchgang.Name;
            Kriterien = durchgang.Kriterien.OrderBy(k => k.Name).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || BewertetePunkte.Count == 0)
            {
                ErrorMessage = "Bitte gib für alle Kriterien eine Bewertung ab.";
                return await OnGetAsync();
            }

            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null)
            {
                ErrorMessage = "Juror nicht gefunden.";
                return RedirectToPage("/Bewertung/Start");
            }

            foreach (var kv in BewertetePunkte)
            {
                var kriterium = await _context.Kriterien.FindAsync(kv.Key);
                if (kriterium == null) continue;

                var gewichteteNote = kv.Value * kriterium.Gewichtung;

                var bewertung = new BBQHub.Domain.Entities.Bewertung
                {
                    JurorId = juror.Id,
                    DurchgangId = DurchgangId,
                    KriteriumId = kriterium.Id,
                    Punkte = kv.Value,
                    GewichteteNote = gewichteteNote,
                    TeamId = TeamId,
                    SpontanTeilnahmeId = TeilnehmerId
                };

                _context.Bewertungen.Add(bewertung);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Bewertung/Abgeschlossen");
        }
    }
}
