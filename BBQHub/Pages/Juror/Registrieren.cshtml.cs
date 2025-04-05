using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Application.Juroren.Dtos;
using BBQHub.Application.Juroren.Services;

namespace BBQHub.Pages.Juror
{
    public class RegistrierenModel : PageModel
    {
        private readonly IJurorService _jurorService;

        public RegistrierenModel(IJurorService jurorService)
        {
            _jurorService = jurorService;
        }

        public string? SuccessMessage { get; set; }

        [BindProperty]
        public JurorDto Input { get; set; } = new();

        [BindProperty]
        public string Signature { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public List<string> AvailableCountries { get; set; } = new()
        {
            "Schweiz", "Deutschland", "Österreich", "Italien", "Frankreich"
        };

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var result = await _jurorService.RegisterAsync(Input);

                if (!result.Success)
                {
                    ErrorMessage = result.ErrorMessage;
                    return Page();
                }

                SuccessMessage = $"Danke für deine Anmeldung, {Input.FirstName}!";
                ModelState.Clear();
                Input = new();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ein unerwarteter Fehler ist aufgetreten. {ex}";
                // Optional: Logging via IAppLogger hier einbauen
                return Page();
            }
        }
    }
}
