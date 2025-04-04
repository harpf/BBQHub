using BBQHub.Domain.Entities;
using BBQHub.Hubs;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Bewertung
{
    public class FormularModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventMonitorHub> _hubContext;

        public FormularModel(ApplicationDbContext context, IHubContext<EventMonitorHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty(SupportsGet = true)]
        public int JuryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DurchgangId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TeamId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TeilnehmerId { get; set; }

        [BindProperty]
        public Dictionary<int, int> Punkte { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public string EventName { get; set; } = "";
        public string DurchgangName { get; set; } = "";
        public List<Kriterium> Kriterien { get; set; } = new();

        public bool IstTeamBewertung => TeamId != null;

        public async Task<IActionResult> OnGetAsync()
        {
            var eventObj = await _context.Events.FindAsync(EventId);
            var durchgang = await _context.Durchgaenge.FindAsync(DurchgangId);
            if (eventObj == null || durchgang == null)
            {
                return RedirectToPage("/Bewertung/Start");
            }

            EventName = eventObj.Name;
            DurchgangName = durchgang.Name;

            Kriterien = await _context.Kriterien
                .Where(k => k.DurchgangId == DurchgangId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null)
                return RedirectToPage("/Bewertung/Start");

            // Duplikatsprüfung
            bool exists = await _context.Bewertungen.AnyAsync(b =>
                b.JurorId == juror.Id &&
                b.DurchgangId == DurchgangId &&
                b.TeamId == TeamId &&
                b.SpontanTeilnahmeId == TeilnehmerId);

            if (exists)
            {
                ErrorMessage = "Du hast diese Bewertung bereits abgegeben.";
                await OnGetAsync(); // Für EventName etc.
                return Page();
            }

            var kriterien = await _context.Kriterien
                .Where(k => k.DurchgangId == DurchgangId)
                .ToListAsync();

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
                    TeamId = TeamId,
                    SpontanTeilnahmeId = TeilnehmerId
                });
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UpdateEventMonitor");

            if (IstTeamBewertung)
            {
                // Prüfen, ob noch Teams verfügbar sind
                var bewertet = await _context.Bewertungen
                    .Where(b => b.JurorId == juror.Id && b.DurchgangId == DurchgangId)
                    .Select(b => b.TeamId)
                    .Distinct()
                    .ToListAsync();

                var nochVerfuegbareTeams = await _context.EventTeamAssignments
                    .Where(a => a.EventId == EventId && !bewertet.Contains(a.TeamId))
                    .ToListAsync();

                if (nochVerfuegbareTeams.Any())
                {
                    return RedirectToPage("/Bewertung/TeamCode", new
                    {
                        juryId = JuryId,
                        eventId = EventId,
                        durchgangId = DurchgangId
                    });
                }
            }
            else
            {
                // Prüfen, ob noch Teilnehmer verfügbar sind
                var bewertet = await _context.Bewertungen
                    .Where(b => b.JurorId == juror.Id && b.DurchgangId == DurchgangId)
                    .Select(b => b.SpontanTeilnahmeId)
                    .Distinct()
                    .ToListAsync();

                var nochVerfuegbareTeilnehmer = await _context.spontanTeilnahmen
                    .Where(t => t.DurchgangId == DurchgangId && !bewertet.Contains(t.Id))
                    .ToListAsync();

                if (nochVerfuegbareTeilnehmer.Any())
                {
                    return RedirectToPage("/Bewertung/TeilnehmerCode", new
                    {
                        juryId = JuryId,
                        eventId = EventId,
                        durchgangId = DurchgangId
                    });
                }
            }

            return RedirectToPage("/Bewertung/Danke");
        }
    }
}