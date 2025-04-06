using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Bewertung
{
    public class StartModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StartModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int JuryId { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var juror = await _context.Juroren
                .FirstOrDefaultAsync(j => j.JuryId == JuryId);

            if (juror == null)
            {
                ErrorMessage = "Diese Juror-ID wurde nicht gefunden.";
                return Page();
            }

            if (JuryId == 0)
            {
                ModelState.AddModelError("JuryId", "Bitte eine gültige Juror-ID eingeben.");
                return Page();
            }


            return RedirectToPage("/Bewertung/EventAuswahl", new { juryId = JuryId });
        }

    }
}