using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Admin.Events
{
    [Authorize(Roles = "Admin,EventOrganizer")]
    public class VerwaltenTeilnehmerModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VerwaltenTeilnehmerModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Event? Event { get; set; }
        public List<Durchgang> Durchgaenge { get; set; } = new();
        public Dictionary<int, List<SpontanTeilnahme>> TeilnehmerByDurchgang { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Event = await _context.Events
                .Include(e => e.Durchgaenge)
                .FirstOrDefaultAsync(e => e.Id == EventId);

            if (Event == null)
                return NotFound();

            Durchgaenge = Event.Durchgaenge.OrderBy(d => d.Durchgangsnummer).ToList();

            var teilnehmer = await _context.spontanTeilnahmen
                .Where(t => Durchgaenge.Select(d => d.Id).Contains(t.DurchgangId))
                .Include(t => t.Durchgang)
                .ToListAsync();

            TeilnehmerByDurchgang = teilnehmer
                .GroupBy(t => t.DurchgangId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return Page();
        }

        public async Task<IActionResult> OnPostVerschiebenAsync(int TeilnehmerId, int NeuerDurchgangId)
        {
            var teilnehmer = await _context.spontanTeilnahmen.FindAsync(TeilnehmerId);
            if (teilnehmer == null) return NotFound();

            teilnehmer.DurchgangId = NeuerDurchgangId;
            await _context.SaveChangesAsync();

            // EventId aus zugehörigem Durchgang ermitteln
            var durchgang = await _context.Durchgaenge.FindAsync(NeuerDurchgangId);
            var eventId = durchgang?.EventId ?? 0;

            return RedirectToPage(new { EventId = eventId });
        }

        public async Task<IActionResult> OnPostLoeschenAsync(int TeilnehmerId)
        {
            var teilnehmer = await _context.spontanTeilnahmen.FindAsync(TeilnehmerId);
            if (teilnehmer == null) return NotFound();

            // EventId aus zugehörigem Durchgang holen
            var durchgang = await _context.Durchgaenge.FindAsync(teilnehmer.DurchgangId);
            var eventId = durchgang?.EventId ?? 0;

            _context.spontanTeilnahmen.Remove(teilnehmer);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { EventId = eventId });
        }
    }
}
