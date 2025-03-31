using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Admin.Events
{
    [Authorize(Roles = "Admin,EventOrganizer")]
    public class BewertungenModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BewertungenModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // EventId

        public Domain.Entities.Event? Event { get; set; }
        public List<Team> Teams { get; set; } = new();
        public List<Durchgang> Durchgaenge { get; set; } = new();
        public Dictionary<(int TeamId, int DurchgangId, int KriteriumId), List<BBQHub.Domain.Entities.Bewertung>> BewertungenMap { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Event = await _context.Events
                .Include(e => e.Durchgaenge)
                .ThenInclude(d => d.Kriterien)
                .FirstOrDefaultAsync(e => e.Id == Id);

            if (Event == null) return NotFound();

            Durchgaenge = Event.Durchgaenge;

            Teams = await _context.EventTeamAssignments
                .Include(a => a.Team)
                .Where(a => a.EventId == Id)
                .Select(a => a.Team)
                .ToListAsync();

            var bewertungen = await _context.Bewertungen
                .Where(b => b.Durchgang.EventId == Id)
                .Include(b => b.Kriterium)
                .Include(b => b.Juror)
                .ToListAsync();

            foreach (var b in bewertungen)
            {
                var key = (b.TeamId, b.DurchgangId, b.KriteriumId);
                if (!BewertungenMap.ContainsKey(key))
                    BewertungenMap[key] = new List<BBQHub.Domain.Entities.Bewertung>();
                BewertungenMap[key].Add(b);
            }

            return Page();
        }

        public double BerechneStandardabweichung(List<int> werte)
        {
            if (werte.Count == 0) return 0;
            double avg = werte.Average();
            double sum = werte.Sum(w => Math.Pow(w - avg, 2));
            return Math.Sqrt(sum / werte.Count);
        }
    }
}
