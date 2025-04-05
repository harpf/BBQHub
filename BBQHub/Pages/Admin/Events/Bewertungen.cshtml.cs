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

        public Event? Event { get; set; }

        public List<Team> Teams { get; set; } = new();
        public List<SpontanTeilnahme> SpontanTeilnahmen { get; set; } = new();
        public List<Durchgang> Durchgaenge { get; set; } = new();

        // Key: (TeamId or TeilnehmerId, DurchgangId, KriteriumId)
        public Dictionary<(int, int, int), List<BBQHub.Domain.Entities.Bewertung>> BewertungenMap { get; set; } = new();

        public bool IstSpontanEvent => Event?.Typ == EventType.SpontanTeilnahme;

        public async Task<IActionResult> OnGetAsync()
        {
            Event = await _context.Events
                .Include(e => e.Durchgaenge)
                    .ThenInclude(d => d.Kriterien)
                .FirstOrDefaultAsync(e => e.Id == Id);

            if (Event == null) return NotFound();

            Durchgaenge = Event.Durchgaenge.OrderBy(d => d.Durchgangsnummer).ToList();
            var durchgangIds = Durchgaenge.Select(d => d.Id).ToList();

            if (IstSpontanEvent)
            {
                SpontanTeilnahmen = await _context.spontanTeilnahmen
                    .Where(t => durchgangIds.Contains(t.DurchgangId))
                    .ToListAsync();
            }
            else
            {
                Teams = await _context.EventTeamAssignments
                    .Where(a => a.EventId == Id)
                    .Include(a => a.Team)
                    .Select(a => a.Team)
                    .ToListAsync();
            }

            var bewertungen = await _context.Bewertungen
                .Where(b => durchgangIds.Contains(b.DurchgangId))
                .Include(b => b.Kriterium)
                .Include(b => b.Juror)
                .ToListAsync();

            foreach (var b in bewertungen)
            {
                // Schlüssel ist entweder TeamId oder SpontanTeilnahmeId
                int? entityId = b.TeamId ?? b.SpontanTeilnahmeId;

                if (!entityId.HasValue)
                    continue;

                var key = (entityId.Value, b.DurchgangId, b.KriteriumId);

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