using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBQHub.Pages.Bewertung
{
    public class DankeModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int JuryId { get; set; }

        public IActionResult OnGet()
        {
            if (JuryId <= 0)
            {
                return RedirectToPage("/Bewertung/Start");
            }

            // Optional: falls du die Seite direkt überspringen willst
            return RedirectToPage("/Bewertung/EventAuswahl", new { juryId = JuryId });
        }
    }
}