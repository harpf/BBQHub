using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Admin.Events
{
    [Authorize(Roles = "Admin,EventOrganizer")]
    public class MonitorModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MonitorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Event> Events { get; set; } = new();
        public Dictionary<int, int> TotalBewertungen { get; set; } = new();
        public Dictionary<int, int> ErwarteteBewertungen { get; set; } = new();
        public Dictionary<int, List<JurorStatus>> JurorAktivität { get; set; } = new();

        public class JurorStatus
        {
            public string Name { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public async Task OnGetAsync()
        {
            Events = await _context.Events
                .Where(e => e.Status == EventStatus.Aktiv)
                .Include(e => e.Durchgaenge)
                .ThenInclude(d => d.Kriterien)
                .ToListAsync();

            foreach (var ev in Events)
            {
                var durchgangIds = ev.Durchgaenge.Select(d => d.Id).ToList();
                var kriterien = ev.Durchgaenge.SelectMany(d => d.Kriterien).ToList();

                var teamZuweisungen = await _context.EventTeamAssignments
                    .CountAsync(a => a.EventId == ev.Id);
                var erwarteteBewertungen = teamZuweisungen * kriterien.Count;

                ErwarteteBewertungen[ev.Id] = erwarteteBewertungen == 0 ? 1 : erwarteteBewertungen;

                var abgegeben = await _context.Bewertungen
                    .Where(b => durchgangIds.Contains(b.DurchgangId))
                    .CountAsync();
                TotalBewertungen[ev.Id] = abgegeben;

                var jurorBewertungen = await _context.Bewertungen
                    .Where(b => durchgangIds.Contains(b.DurchgangId))
                    .GroupBy(b => b.JurorId)
                    .Select(g => new { JurorId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var juroren = await _context.Juroren.ToListAsync();
                JurorAktivität[ev.Id] = jurorBewertungen.Select(j => new JurorStatus
                {
                    Name = juroren.FirstOrDefault(x => x.Id == j.JurorId)?.FirstName ?? "Unbekannt",
                    Count = j.Count
                }).ToList();
            }
        }
    }
}