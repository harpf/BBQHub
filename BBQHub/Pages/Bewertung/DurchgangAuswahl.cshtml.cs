using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Bewertung
{
    public class DurchgangAuswahlModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DurchgangAuswahlModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int JuryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }
        [BindProperty]
        public int SelectedDurchgangId { get; set; }

        public string? EventName { get; set; }

        public List<BBQHub.Domain.Entities.Durchgang> Durchgaenge { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null)
                return RedirectToPage("/Bewertung/Start");

            var ev = await _context.Events.FindAsync(EventId);
            if (ev == null)
                return RedirectToPage("/Bewertung/EventAuswahl", new { juryId = JuryId });

            EventName = ev.Name;

            Durchgaenge = await _context.Durchgaenge
                .Where(d => d.EventId == EventId)
                .OrderBy(d => d.Durchgangsnummer)
                .ToListAsync();

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (SelectedDurchgangId == 0)
            {
                ModelState.AddModelError("", "Bitte wähle einen Durchgang aus.");
                return await OnGetAsync(); // damit die Seite korrekt neu aufgebaut wird
            }

            var durchgang = await _context.Durchgaenge
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Id == SelectedDurchgangId);

            if (durchgang == null || durchgang.Event == null)
                return NotFound();

            if (durchgang.Event.Typ == BBQHub.Domain.Entities.EventType.SpontanTeilnahme)
            {
                return RedirectToPage("/Bewertung/TeilnehmerCode", new
                {
                    juryId = JuryId,
                    eventId = EventId,
                    durchgangId = SelectedDurchgangId
                });
            }
            else
            {
                return RedirectToPage("/Bewertung/TeamCode", new
                {
                    juryId = JuryId,
                    eventId = EventId,
                    durchgangId = SelectedDurchgangId
                });
            }
        }

    }
}