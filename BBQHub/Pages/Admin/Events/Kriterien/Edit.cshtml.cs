using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Admin.Events.Kriterien
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Kriterium Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var kriterium = await _context.Kriterien.FindAsync(id);
            if (kriterium == null) return NotFound();

            Input = kriterium;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var existing = await _context.Kriterien
                .Include(k => k.Durchgang) // für Redirect
                .FirstOrDefaultAsync(k => k.Id == Input.Id);

            if (existing == null)
                return NotFound();

            // Alle Bewertungen zu diesem Kriterium laden
            var abhängigeBewertungen = await _context.Bewertungen
                .Where(b => b.KriteriumId == existing.Id)
                .ToListAsync();

            // Bewertungen entfernen
            _context.Bewertungen.RemoveRange(abhängigeBewertungen);

            // Dann das Kriterium entfernen
            _context.Kriterien.Remove(existing);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/EditEvent", new { id = existing.Durchgang.EventId });
        }

        private int GetEventIdFromDurchgang(int durchgangId)
        {
            var durchgang = _context.Durchgaenge.FirstOrDefault(d => d.Id == durchgangId);
            return durchgang?.EventId ?? 0;
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {

            var existing = await _context.Kriterien.FindAsync(Input.Id);
            if (existing == null) return NotFound();

            existing.Name = Input.Name;
            existing.MaxWert = Input.MaxWert;
            existing.Gewichtung = Input.Gewichtung;
            existing.ZaehltZurGesamtwertung = Input.ZaehltZurGesamtwertung;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/EditEvent", new { id = GetEventIdFromDurchgang(existing.DurchgangId) });
        }
    }
}
