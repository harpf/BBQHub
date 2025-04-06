using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BBQHub.Pages.Teilnahme
{
    public class DatenschutzModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vorname ist erforderlich.")]
        public string Vorname { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Nachname ist erforderlich.")]
        public string Nachname { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Datum ist erforderlich.")]
        [DataType(DataType.Date)]
        public DateTime Datum { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "Die Unterschrift wird benötigt.")]
        public string UnterschriftBild { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (EventId <= 0)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Übergabe via TempData an Registrierungsseite
            TempData["DatenschutzUnterschrift"] = UnterschriftBild;
            TempData["Vorname"] = Vorname;
            TempData["Nachname"] = Nachname;
            TempData["Datum"] = Datum.ToString("o");

            return RedirectToPage("/Teilnahme/Registrieren", new { eventId = EventId });
        }
    }
}