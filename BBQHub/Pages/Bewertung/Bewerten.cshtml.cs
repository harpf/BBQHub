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

        public List<Kriterium> Kriterien { get; set; } = new();

        [BindProperty]
        public Dictionary<int, int> Punkte { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null)
            {
                ErrorMessage = "Juror nicht gefunden.";
                return Page();
            }

            // Token über EventTeamAssignment prüfen
            var assignment = await _context.EventTeamAssignments
                .Include(a => a.Team)
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.Token == Token);

            if (assignment == null)
            {
                ErrorMessage = "Ungültiger Team-Code.";
                return Page();
            }

            Team = assignment.Team;

            // Doppelte Bewertung verhindern
            var schonBewertet = await _context.Bewertungen
                .AnyAsync(b =>
                    b.JurorId == juror.Id &&
                    b.DurchgangId == DurchgangId &&
                    b.TeamId == assignment.TeamId);

            if (schonBewertet)
            {
                ErrorMessage = "Du hast dieses Team in diesem Durchgang bereits bewertet.";
                return Page();
            }

            // Kriterien laden
            Kriterien = await _context.Kriterien
                .Where(k => k.DurchgangId == DurchgangId)
                .ToListAsync();

            // Bewertungen speichern
            foreach (var kriterium in Kriterien)
            {
                var punkt = Punkte.TryGetValue(kriterium.Id, out var value) ? value : 0;
                var gewichteteNote = punkt * kriterium.Gewichtung;

                _context.Bewertungen.Add(new Domain.Entities.Bewertung
                {
                    JurorId = juror.Id,
                    DurchgangId = DurchgangId,
                    TeamId = assignment.TeamId,
                    KriteriumId = kriterium.Id,
                    Punkte = punkt,
                    GewichteteNote = gewichteteNote
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Bewertung/Danke");
        }

    }
}
