using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Bewertung
{
    public class TeilnehmerCodeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TeilnehmerCodeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string JuryId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DurchgangId { get; set; }

        public string EventName { get; set; } = string.Empty;
        public string DurchgangName { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedTeilnehmerId { get; set; }

        public List<SpontanTeilnahme> VerfügbareTeilnahmen { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lade den Durchgang und hole den zugehörigen Event separat
            var durchgang = await _context.Durchgaenge
                .FirstOrDefaultAsync(d => d.Id == DurchgangId);

            if (durchgang == null)
                return NotFound();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == durchgang.EventId);
            if (ev == null)
                return NotFound();

            EventName = ev.Name;
            DurchgangName = durchgang.Name;

            // Lade alle Teilnehmer für diesen Durchgang
            VerfügbareTeilnahmen = await _context.spontanTeilnahmen
                .Where(t => t.DurchgangId == DurchgangId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (SelectedTeilnehmerId == 0)
            {
                ErrorMessage = "Bitte wähle einen Teilnehmer aus.";
                return await OnGetAsync();
            }

            // Weiterleitung zur Bewertungsseite mit TeilnehmerId
            return RedirectToPage("/Bewertung/Formular", new
            {
                juryId = JuryId,
                eventId = EventId,
                durchgangId = DurchgangId,
                teilnehmerId = SelectedTeilnehmerId
            });
        }
    }
}