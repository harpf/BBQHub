using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Bewertung
{
    public class EventAuswahlModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EventAuswahlModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int JuryId { get; set; }

        public List<Event> Events { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var juror = await _context.Juroren.FirstOrDefaultAsync(j => j.JuryId == JuryId);
            if (juror == null)
                return RedirectToPage("/Bewertung/Start");

            Events = await _context.Events
                .Where(e => e.Status == EventStatus.Aktiv)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return Page();
        }
    }
}