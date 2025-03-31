using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Admin.Events
{
    public class EditBewertungModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditBewertungModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int TeamId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DurchgangId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int KriteriumId { get; set; }

        public List<BBQHub.Domain.Entities.Bewertung> Bewertungen { get; set; } = new();
        public string TeamName { get; set; } = "";
        public string KriteriumName { get; set; } = "";
        public string DurchgangName { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            Bewertungen = await _context.Bewertungen
                .Include(b => b.Juror)
                .Include(b => b.Kriterium)
                .Include(b => b.Durchgang)
                .Where(b => b.TeamId == TeamId && b.DurchgangId == DurchgangId && b.KriteriumId == KriteriumId)
                .ToListAsync();

            if (!Bewertungen.Any()) return NotFound();

            TeamName = (await _context.Teams.FindAsync(TeamId))?.Name ?? "Unbekannt";
            KriteriumName = Bewertungen.First().Kriterium.Name;
            DurchgangName = Bewertungen.First().Durchgang.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<BBQHub.Domain.Entities.Bewertung> bewertungen)
        {
            foreach (var b in bewertungen)
            {
                var original = await _context.Bewertungen.FindAsync(b.Id);
                if (original != null)
                {
                    original.Punkte = b.Punkte;
                    var kriterium = await _context.Kriterien.FindAsync(original.KriteriumId);
                    original.GewichteteNote = original.Punkte * (kriterium?.Gewichtung ?? 1);
                }
            }

            await _context.SaveChangesAsync();

            var eventId = await _context.Durchgaenge
                .Where(d => d.Id == DurchgangId)
                .Select(d => d.EventId)
                .FirstOrDefaultAsync();

            return RedirectToPage("/Admin/Events/Bewertungen", new { id = eventId });
        }
    }
}