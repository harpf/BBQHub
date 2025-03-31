using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Bewertung
{
    public class TeamCodeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TeamCodeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int JuryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DurchgangId { get; set; }
        public string? EventName { get; set; }
        public string? DurchgangName { get; set; }


        [BindProperty]
        public int SelectedTeamId { get; set; }

        public List<EventTeamAssignment> VerfügbareTeams { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null)
                return RedirectToPage("/Bewertung/Start");

            var bereitsBewerteteTeamIds = await _context.Bewertungen
                .Where(b => b.JurorId == juror.Id && b.DurchgangId == DurchgangId)
                .Select(b => b.TeamId)
                .ToListAsync();

            VerfügbareTeams = await _context.EventTeamAssignments
                .Include(a => a.Team)
                .Where(a => a.EventId == EventId && !bereitsBewerteteTeamIds.Contains(a.TeamId))
                .ToListAsync();

            var eventData = await _context.Events.FirstOrDefaultAsync(e => e.Id == EventId);
            if (eventData != null)
                EventName = eventData.Name;

            var durchgang = await _context.Durchgaenge.FirstOrDefaultAsync(d => d.Id == DurchgangId);
            if (durchgang != null)
                DurchgangName = durchgang.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var assignment = await _context.EventTeamAssignments
                .FirstOrDefaultAsync(a => a.EventId == EventId && a.TeamId == SelectedTeamId);

            if (assignment == null)
            {
                ErrorMessage = "Ungültige Auswahl.";
                return await OnGetAsync(); // lade erneut die Liste
            }

            return RedirectToPage("/Bewertung/Formular", new
            {
                juryId = JuryId,
                eventId = EventId,
                durchgangId = DurchgangId,
                teamId = assignment.TeamId
            });
        }
    }
}