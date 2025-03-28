using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Juror
{
    public class RegistrierenModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegistrierenModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public BBQHub.Domain.Entities.Juror Input { get; set; } = new();

        [BindProperty]
        public string Signature { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public List<string> AvailableCountries { get; set; } = new()
        {
            "Schweiz", "Deutschland", "Österreich", "Italien", "Frankreich"
        };

        public async Task<IActionResult> OnPostAsync()
        {
            if (await _context.Juroren.AnyAsync(j => j.JuryId == Input.JuryId))
            {
                ErrorMessage = "Diese Jury ID ist bereits vergeben.";
                return Page();
            }

            if (!string.IsNullOrEmpty(Input.Email) &&
                await _context.Juroren.AnyAsync(j => j.Email == Input.Email))
            {
                ErrorMessage = "Diese E-Mail-Adresse ist bereits registriert.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Vereinslocation))
            {
                ModelState.AddModelError("Input.Vereinslocation", "Bitte ein Land auswählen.");
            }

            _context.Juroren.Add(Input);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Danke für deine Anmeldung, {Input.FirstName}!";

            ModelState.Clear();
            Input = new();
            return Page();

        }
    }
}
